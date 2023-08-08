using DFC.Common.Standard.Logging;
using DFC.GeoCoding.Standard.AzureMaps.Model;
using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using NCS.DSS.Address.Cosmos.Helper;
using NCS.DSS.Address.GeoCoding;
using NCS.DSS.Address.Models;
using NCS.DSS.Address.PatchAddressHttpTrigger.Service;
using NCS.DSS.Address.Validation;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace NCS.DSS.Address.PatchAddressHttpTrigger.Function
{
    public class PatchAddressHttpTrigger
    {
        private IResourceHelper _resourceHelper;
        private IValidate _validate;
        private IPatchAddressHttpTriggerService _addressPatchService;
        private ILoggerHelper _loggerHelper;
        private IHttpRequestHelper _httpRequestHelper;
        private IHttpResponseMessageHelper _httpResponseMessageHelper;
        private IJsonHelper _jsonHelper;
        private IGeoCodingService _geoCodingService;

        public PatchAddressHttpTrigger(IResourceHelper resourceHelper,
            IValidate validate,
            IPatchAddressHttpTriggerService addressPatchService,
            ILoggerHelper loggerHelper,
            IHttpRequestHelper httpRequestHelper,
            IHttpResponseMessageHelper httpResponseMessageHelper,
            IJsonHelper jsonHelper,
            IGeoCodingService geoCodingService)
        {
            _resourceHelper = resourceHelper;
            _validate = validate;
            _addressPatchService = addressPatchService;
            _loggerHelper = loggerHelper;
            _httpRequestHelper = httpRequestHelper;
            _httpResponseMessageHelper = httpResponseMessageHelper;
            _jsonHelper = jsonHelper;
            _geoCodingService = geoCodingService;
        }

        [FunctionName("Patch")]
        [ProducesResponseType(typeof(AddressPatch), 200)]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Address Updated", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Address does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = 422, Description = "Address validation error(s)", ShowSchema = false)]
        [Display(Name = "Patch", Description = "Ability to update an existing address.")]
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "Customers/{customerId}/Addresses/{addressId}")] HttpRequest req, ILogger log, string customerId, string addressId)
        {
            _loggerHelper.LogMethodEnter(log);

            var correlationId = _httpRequestHelper.GetDssCorrelationId(req);
            if (string.IsNullOrEmpty(correlationId))
                log.LogInformation("Unable to locate 'DssCorrelationId; in request header");

            if (!Guid.TryParse(correlationId, out var correlationGuid))
            {
                log.LogInformation("Unable to Parse 'DssCorrelationId' to a Guid");
                correlationGuid = Guid.NewGuid();
            }

            var touchpointId = _httpRequestHelper.GetDssTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, "Unable to locate 'APIM-TouchpointId' in request header");
                return _httpResponseMessageHelper.BadRequest();
            }

            var ApimURL = _httpRequestHelper.GetDssApimUrl(req);
            if (string.IsNullOrEmpty(ApimURL))
            {
                log.LogInformation("Unable to locate 'apimurl' in request header");
                return _httpResponseMessageHelper.BadRequest();
            }

            var subContractorId = _httpRequestHelper.GetDssSubcontractorId(req);
            if (string.IsNullOrEmpty(subContractorId))
                _loggerHelper.LogInformationMessage(log, correlationGuid, "Unable to locate 'SubContractorId' in request header");

            _loggerHelper.LogInformationMessage(log, correlationGuid, "Patch Address C# HTTP trigger function  processed a request. By Touchpoint " + touchpointId);

            if (!Guid.TryParse(customerId, out var customerGuid))
                return _httpResponseMessageHelper.BadRequest(customerGuid);

            if (!Guid.TryParse(addressId, out var addressGuid))
                return _httpResponseMessageHelper.BadRequest(addressGuid);

            AddressPatch addressPatchRequest;

            try
            {
                addressPatchRequest = await _httpRequestHelper.GetResourceFromRequest<AddressPatch>(req);
            }
            catch (JsonException ex)
            {
                return _httpResponseMessageHelper.UnprocessableEntity(ex);
            }

            if (addressPatchRequest == null)
                return _httpResponseMessageHelper.UnprocessableEntity(req);

            addressPatchRequest.SetIds(touchpointId, subContractorId);

            var errors = _validate.ValidateResource(addressPatchRequest, false);

            if (errors != null && errors.Any())
                return _httpResponseMessageHelper.UnprocessableEntity(errors);

            _loggerHelper.LogInformationMessage(log, correlationGuid, "Attempting to get long and lat for postcode");

            if (!string.IsNullOrEmpty(addressPatchRequest.PostCode))
            {
                Position position;

                try
                {
                    var postcode = addressPatchRequest.PostCode.Replace(" ", string.Empty);
                    position = await _geoCodingService.GetPositionForPostcodeAsync(postcode);
                }
                catch (Exception e)
                {
                    _loggerHelper.LogException(log, correlationGuid, string.Format("Unable to get long and lat for postcode: {0}", addressPatchRequest.PostCode), e);
                    throw;
                }

                addressPatchRequest.SetLongitudeAndLatitude(position);
            }

            var doesCustomerExist = await _resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
                return _httpResponseMessageHelper.NoContent(customerGuid);

            var isCustomerReadOnly = await _resourceHelper.IsCustomerReadOnly(customerGuid);

            if (isCustomerReadOnly)
                return _httpResponseMessageHelper.Forbidden(customerGuid);

            var address = await _addressPatchService.GetAddressForCustomerAsync(customerGuid, addressGuid);

            if (string.IsNullOrEmpty(address))
                return _httpResponseMessageHelper.NoContent(customerGuid);

            _loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Attempting to get patch customer resource {0}", customerGuid));
            var patchedAddress = _addressPatchService.PatchResource(address, addressPatchRequest,log);

            if (patchedAddress == null)
                return _httpResponseMessageHelper.NoContent(addressGuid);

            var updatedAddress = await _addressPatchService.UpdateCosmosAsync(patchedAddress, addressGuid,log);

            if (updatedAddress != null)
                await _addressPatchService.SendToServiceBusQueueAsync(updatedAddress, customerGuid, ApimURL);

            return updatedAddress == null ?
                _httpResponseMessageHelper.BadRequest(addressGuid) :
                _httpResponseMessageHelper.Ok(_jsonHelper.SerializeObjectAndRenameIdProperty(updatedAddress, "id", "AddressId"));
        }
    }
}