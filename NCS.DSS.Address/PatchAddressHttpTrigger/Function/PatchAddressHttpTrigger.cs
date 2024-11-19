using DFC.GeoCoding.Standard.AzureMaps.Model;
using DFC.HTTP.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.Address.Cosmos.Helper;
using NCS.DSS.Address.GeoCoding;
using NCS.DSS.Address.Helpers;
using NCS.DSS.Address.Models;
using NCS.DSS.Address.PatchAddressHttpTrigger.Service;
using NCS.DSS.Address.Validation;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace NCS.DSS.Address.PatchAddressHttpTrigger.Function
{
    public class PatchAddressHttpTrigger
    {
        private readonly IResourceHelper _resourceHelper;
        private readonly IValidate _validate;
        private readonly IPatchAddressHttpTriggerService _addressPatchService;
        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly IGeoCodingService _geoCodingService;
        private readonly ILogger _logger;
        private readonly IDynamicHelper _dynamicHelper;

        public PatchAddressHttpTrigger(IResourceHelper resourceHelper,
            IValidate validate,
            IPatchAddressHttpTriggerService addressPatchService,
            IHttpRequestHelper httpRequestHelper,
            IGeoCodingService geoCodingService,
            ILogger<PatchAddressHttpTrigger> logger,
            IDynamicHelper dynamicHelper)
        {
            _resourceHelper = resourceHelper;
            _validate = validate;
            _addressPatchService = addressPatchService;
            _httpRequestHelper = httpRequestHelper;
            _geoCodingService = geoCodingService;
            _logger = logger;
            _dynamicHelper = dynamicHelper;
        }

        [Function("Patch")]
        [ProducesResponseType(typeof(AddressPatch), 200)]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Address Updated", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Address does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = 422, Description = "Address validation error(s)", ShowSchema = false)]
        [Display(Name = "Patch", Description = "Ability to update an existing address.")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "Customers/{customerId}/Addresses/{addressId}")] HttpRequest req, string customerId, string addressId)
        {
            _logger.LogInformation($"Started Executing Address PATCH Request for an AddressId [{addressId}] and CustomerId [{customerId}]");

            var correlationId = _httpRequestHelper.GetDssCorrelationId(req);
            if (string.IsNullOrEmpty(correlationId))
                _logger.LogWarning("Unable to locate 'DssCorrelationId; in request header");

            if (!Guid.TryParse(correlationId, out var correlationGuid))
            {
                correlationGuid = Guid.NewGuid();
                _logger.LogWarning($"Unable to Parse 'DssCorrelationId' to a Guid. New Guid Generated");
            }

            _logger.LogInformation($" 'DssCorrelationId' is [{correlationGuid}]");

            var touchpointId = _httpRequestHelper.GetDssTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
                return ReturnBadRequest("Unable to locate 'APIM-TouchpointId' in request header.");

            var ApimURL = _httpRequestHelper.GetDssApimUrl(req);
            if (string.IsNullOrEmpty(ApimURL))
                return ReturnBadRequest("Unable to locate 'apimurl' in request header.");

            var subContractorId = _httpRequestHelper.GetDssSubcontractorId(req);
            if (string.IsNullOrEmpty(subContractorId))
                _logger.LogInformation("Unable to locate 'SubContractorId' in request header. Continuing Patch Process");

            _logger.LogInformation("Patch Address C# HTTP trigger function  processed a request. By Touchpoint " + touchpointId);

            if (!Guid.TryParse(customerId, out var customerGuid))
                return ReturnBadRequest("Unable to Parse customerId to Guid.", customerGuid);

            if (!Guid.TryParse(addressId, out var addressGuid))
                return ReturnBadRequest("Unable to Parse Address Id to Guid.", addressGuid);

            AddressPatch addressPatchRequest;

            try
            {
                addressPatchRequest = await _httpRequestHelper.GetResourceFromRequest<AddressPatch>(req);
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Failed to Prase Json object. Response Code [{HttpStatusCode.UnprocessableContent}]. Exception Message: [{ex.Message}]");
                return new UnprocessableEntityObjectResult(_dynamicHelper.ExcludeProperty(ex, ["TargetSite"]));
            }

            if (addressPatchRequest == null)
            {
                _logger.LogWarning($"Address Request is Empty. Response Code [{HttpStatusCode.UnprocessableContent}]");
                return new UnprocessableEntityObjectResult(req);
            }

            addressPatchRequest.SetIds(touchpointId, subContractorId);

            try
            {
                addressPatchRequest.PostCode = addressPatchRequest?.PostCode?.TrimEnd().TrimStart();
            }
            catch (Exception e)
            {
                _logger.LogWarning($"Unable to trim the postcode: `{addressPatchRequest.PostCode}`");
                throw;
            }

            var errors = _validate.ValidateResource(addressPatchRequest, false);

            if (errors != null && errors.Any())
                return new UnprocessableEntityObjectResult(errors);

            _logger.LogInformation("Attempting to get long and lat for postcode");

            if (!string.IsNullOrEmpty(addressPatchRequest.PostCode))
            {
                Position position;

                try
                {
                    position = await _geoCodingService.GetPositionForPostcodeAsync(addressPatchRequest.PostCode);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Unable to get long and lat for postcode: {addressPatchRequest.PostCode}");
                    throw;
                }

                addressPatchRequest.SetLongitudeAndLatitude(position);
            }

            var doesCustomerExist = await _resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
                return ReturnNoContent("Customer with given Customer Guid does not exist", customerGuid);

            var isCustomerReadOnly = await _resourceHelper.IsCustomerReadOnly(customerGuid);

            if (isCustomerReadOnly)
            {
                _logger.LogWarning($"Readonly Customer. Response Code [{HttpStatusCode.Forbidden}]");
                return new ObjectResult(customerGuid)
                {
                    StatusCode = (int)HttpStatusCode.Forbidden
                };
            }

            var address = await _addressPatchService.GetAddressForCustomerAsync(customerGuid, addressGuid);

            if (string.IsNullOrEmpty(address))
                return ReturnNoContent("No Address record associated with the given customerId.", customerGuid);

            _logger.LogInformation($"Attempting to get patch customer resource {customerGuid}");

            var patchedAddress = _addressPatchService.PatchResource(address, addressPatchRequest, _logger);

            if (patchedAddress == null)
                return ReturnNoContent($"Related patch address not found for the address guid [{addressGuid}].", addressGuid);


            var updatedAddress = await _addressPatchService.UpdateCosmosAsync(patchedAddress, addressGuid, _logger);

            if (updatedAddress != null)
            {
                await _addressPatchService.SendToServiceBusQueueAsync(updatedAddress, customerGuid, ApimURL);

                _logger.LogInformation($"End of PATCH Address Request. Address Update Complete and Response Status Code [{HttpStatusCode.OK}]");

                return new JsonResult(updatedAddress, new JsonSerializerOptions())
                {
                    StatusCode = (int)HttpStatusCode.OK
                };
            }

            return ReturnBadRequest("End of PATCH Addres Request. Updated Address is null.", addressGuid);

        }
        private IActionResult ReturnBadRequest(string message)
        {
            _logger.LogWarning($"{message}. Response Code [{HttpStatusCode.BadRequest}]");
            return new BadRequestObjectResult(HttpStatusCode.BadRequest);
        }
        private IActionResult ReturnBadRequest(string message, Guid guid)
        {
            _logger.LogWarning($"{message}. Response Code [{HttpStatusCode.BadRequest}]");
            return new BadRequestObjectResult(guid);
        }
        private IActionResult ReturnNoContent(string message, Guid guid)
        {
            _logger.LogWarning($"{message}. Response Code [{HttpStatusCode.NoContent}]");
            return new NoContentResult();
        }
    }
}