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
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;

namespace NCS.DSS.Address.PatchAddressHttpTrigger.Function
{
    public class PatchAddressHttpTrigger
    {
        private readonly IResourceHelper _resourceHelper;
        private readonly IValidate _validate;
        private readonly IPatchAddressHttpTriggerService _addressPatchService;
        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly IGeoCodingService _geoCodingService;
        private readonly ILogger<PatchAddressHttpTrigger> _logger;
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
            _logger.LogInformation("Function {FunctionName} has been invoked", nameof(PatchAddressHttpTrigger));

            var correlationId = _httpRequestHelper.GetDssCorrelationId(req);
            if (string.IsNullOrEmpty(correlationId))
            {
                _logger.LogWarning("Unable to locate 'DssCorrelationId' in request header");
            }

            if (!Guid.TryParse(correlationId, out var correlationGuid))
            {
                correlationGuid = Guid.NewGuid();
                _logger.LogInformation("Unable to parse 'DssCorrelationId' to a Guid. CorrelationId: {CorrelationId}", correlationId);
            }

            var touchpointId = _httpRequestHelper.GetDssTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                _logger.LogInformation("Unable to locate 'TouchpointId' in request header.");
                return new BadRequestObjectResult(HttpStatusCode.BadRequest);
            }

            var apimUrl = _httpRequestHelper.GetDssApimUrl(req);
            if (string.IsNullOrEmpty(apimUrl))
            {
                _logger.LogWarning("Unable to locate 'apimURL' in request header. Correlation GUID: {CorrelationGuid}", correlationGuid);
                return new BadRequestObjectResult(HttpStatusCode.BadRequest);
            }

            var subcontractorId = _httpRequestHelper.GetDssSubcontractorId(req);
            if (string.IsNullOrEmpty(subcontractorId))
            {
                _logger.LogWarning("Unable to locate 'SubcontractorId' in request header. Correlation GUID: {CorrelationGuid}", correlationGuid);
            }

            if (!Guid.TryParse(customerId, out var customerGuid))
            {
                _logger.LogWarning("Unable to parse 'customerId' to a GUID. Customer GUID: {CustomerID}", customerId);
                return new BadRequestObjectResult(customerGuid);
            }

            if (!Guid.TryParse(addressId, out var addressGuid))
            {
                _logger.LogWarning("Unable to parse 'addressId' to a GUID. Address GUID: {AddressID}", addressId);
                return new BadRequestObjectResult(addressGuid);
            }

            _logger.LogInformation("Input validation has succeeded. Touchpoint ID: {TouchpointId}.", touchpointId);

            AddressPatch addressPatchRequest;
            try
            {
                _logger.LogInformation("Attempting to retrieve resource from request. Correlation GUID: {CorrelationGuid}", correlationGuid);
                addressPatchRequest = await _httpRequestHelper.GetResourceFromRequest<AddressPatch>(req);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to parse {addressPatchRequest} from request body. Correlation GUID: {CorrelationGuid}. Exception: {ExceptionMessage}", nameof(addressPatchRequest), correlationGuid, ex.Message);
                return new UnprocessableEntityObjectResult(_dynamicHelper.ExcludeProperty(ex, ["TargetSite"]));
            }

            if (addressPatchRequest == null)
            {
                _logger.LogWarning("{addressPatchRequest} object is NULL. Correlation GUID: {CorrelationGuid}", nameof(addressPatchRequest), correlationGuid);
                return new UnprocessableEntityObjectResult(req);
            }

            _logger.LogInformation("Attempting to set IDs for Address PATCH. Correlation GUID: {CorrelationGuid}", correlationGuid);
            addressPatchRequest.SetIds(touchpointId, subcontractorId);
            _logger.LogInformation("IDs successfully set for Address PATCH. Correlation GUID: {CorrelationGuid}", correlationGuid);

            try
            {
                addressPatchRequest.PostCode = addressPatchRequest?.PostCode?.TrimEnd().TrimStart();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to trim the postcode: {PostCode}. Exception: {ExceptionMessage}", addressPatchRequest.PostCode, ex.Message);
                throw;
            }

            _logger.LogInformation("Attempting to validate {addressPatchRequest} object", nameof(addressPatchRequest));
            var errors = _validate.ValidateResource(addressPatchRequest, false);

            if (errors != null && errors.Any())
            {
                _logger.LogWarning("Falied to validate {addressPatchRequest} object", nameof(addressPatchRequest));
                return new UnprocessableEntityObjectResult(errors);
            }
            _logger.LogInformation("Successfully validated {addressPatchRequest} object", nameof(addressPatchRequest));


            if (!string.IsNullOrEmpty(addressPatchRequest.PostCode))
            {
                Position position;

                try
                {
                    _logger.LogInformation("Attempting to get long and lat for postcode: {Postcode}", addressPatchRequest.PostCode);
                    position = await _geoCodingService.GetPositionForPostcodeAsync(addressPatchRequest.PostCode);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unable to get long and lat for postcode: {PostCode}. Exception: {ExceptionMessage}", addressPatchRequest.PostCode, ex.Message);
                    throw;
                }

                addressPatchRequest.SetLongitudeAndLatitude(position);
                _logger.LogInformation("Successfully set long and lat for postcode: {Postcode}", addressPatchRequest.PostCode);
            }

            _logger.LogInformation("Checking if customer exists. Customer ID: {CustomerId}.", customerGuid);
            var doesCustomerExist = await _resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
            {
                _logger.LogWarning("Customer not found. Customer ID: {CustomerId}.", customerGuid);
                return new NoContentResult();
            }

            _logger.LogInformation("Customer exists. Customer GUID: {CustomerGuid}.", customerGuid);

            _logger.LogInformation("Check if customer is read-only. Customer GUID: {CustomerId}.", customerGuid);
            var isCustomerReadOnly = await _resourceHelper.IsCustomerReadOnly(customerGuid);

            if (isCustomerReadOnly)
            {
                _logger.LogWarning("Customer is read-only. Customer GUID: {CustomerId}.", customerGuid);
                return new ObjectResult(customerGuid.ToString())
                {
                    StatusCode = (int)HttpStatusCode.Forbidden
                };
            }

            _logger.LogInformation("Attempting to get Address for Customer. Customer GUID: {CustomerId}. Address GUID: {AddressId}.", customerGuid, addressGuid);
            var address = await _addressPatchService.GetAddressForCustomerAsync(customerGuid, addressGuid);

            if (string.IsNullOrEmpty(address))
            {
                _logger.LogWarning("Address not found. Customer GUID: {CustomerId}. Address GUID: {AddressId}.", customerGuid, addressGuid);
                return new NoContentResult();
            }

            _logger.LogInformation("Attempting to PATCH Address resource.");
            var patchedAddress = _addressPatchService.PatchResource(address, addressPatchRequest);

            if (patchedAddress == null)
            {
                _logger.LogWarning("Failed to PATCH Address resource.");
                return new NoContentResult();
            }

            _logger.LogInformation("Attempting to update Address in Cosmos DB. Address GUID: {AddressId}", addressGuid);
            var updatedAddress = await _addressPatchService.UpdateCosmosAsync(patchedAddress, addressGuid, _logger);

            if (updatedAddress == null)
            {
                _logger.LogWarning("Failed to update Address in Cosmos DB. Address GUID: {AddressId}", addressGuid);
                _logger.LogInformation("Function {FunctionName} has finished invoking", nameof(PatchAddressHttpTrigger));
                return new BadRequestObjectResult(addressGuid);
            }

            _logger.LogInformation("Address updated successfully in Cosmos DB. Address GUID: {AddressId}", addressGuid);

            _logger.LogInformation("Attempting to send message to Service Bus Namespace. Address GUID: {AddressId}", addressGuid);
            await _addressPatchService.SendToServiceBusQueueAsync(updatedAddress, customerGuid, apimUrl);
            _logger.LogInformation("Successfully sent message to Service Bus. Address GUID: {AddressId}", addressGuid);

            _logger.LogInformation("Function {FunctionName} has finished invoking", nameof(PatchAddressHttpTrigger));

            return new JsonResult(updatedAddress, new JsonSerializerOptions())
            {
                StatusCode = (int)HttpStatusCode.OK
            };
        }
    }
}