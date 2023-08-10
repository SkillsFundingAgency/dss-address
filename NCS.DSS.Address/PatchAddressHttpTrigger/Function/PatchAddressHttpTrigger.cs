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
            log.LogInformation($"Started Executing Address PATCH Request for an AddressId [{addressId}] and CustomerId [{customerId}]");

            var correlationId = _httpRequestHelper.GetDssCorrelationId(req);
            if (string.IsNullOrEmpty(correlationId))
                log.LogWarning("Unable to locate 'DssCorrelationId; in request header");

            if (!Guid.TryParse(correlationId, out var correlationGuid))
            {
                correlationGuid = Guid.NewGuid();
                log.LogWarning($"Unable to Parse 'DssCorrelationId' to a Guid. New Guid Generated");
            }

            log.LogInformation($" 'DssCorrelationId' is [{correlationGuid}]");

            var touchpointId = _httpRequestHelper.GetDssTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
                return ReturnBadRequest( log,"Unable to locate 'APIM-TouchpointId' in request header.");

            var ApimURL = _httpRequestHelper.GetDssApimUrl(req);
            if (string.IsNullOrEmpty(ApimURL))
                return ReturnBadRequest( log,"Unable to locate 'apimurl' in request header.");

            var subContractorId = _httpRequestHelper.GetDssSubcontractorId(req);
            if (string.IsNullOrEmpty(subContractorId))
                log.LogInformation( "Unable to locate 'SubContractorId' in request header. Continuing Patch Process");

            log.LogInformation("Patch Address C# HTTP trigger function  processed a request. By Touchpoint " + touchpointId);

            if (!Guid.TryParse(customerId, out var customerGuid))
                return ReturnBadRequest( log,"Unable to Parse customerId to Guid.",customerGuid);

            if (!Guid.TryParse(addressId, out var addressGuid))
                return ReturnBadRequest( log,"Unable to Parse Address Id to Guid.",addressGuid);

            AddressPatch addressPatchRequest;

            try
            {
                addressPatchRequest = await _httpRequestHelper.GetResourceFromRequest<AddressPatch>(req);
            }
            catch (JsonException ex)
              {
                var unProcessed = _httpResponseMessageHelper.UnprocessableEntity(ex);
                log.LogWarning($"Failed to Prase Json object. Response Code [{unProcessed.StatusCode}]. Exception Message: [{ex.Message}]");
                return unProcessed;
            }

            if (addressPatchRequest == null)
            {
                var unProcessed = _httpResponseMessageHelper.UnprocessableEntity(req);
                log.LogWarning($"Address Request is Empty. Response Code [{unProcessed.StatusCode}]");
                return unProcessed;
            }    

            addressPatchRequest.SetIds(touchpointId, subContractorId);

            var errors = _validate.ValidateResource(addressPatchRequest, false);

            if (errors != null && errors.Any())
                return _httpResponseMessageHelper.UnprocessableEntity(errors);

            log.LogInformation("Attempting to get long and lat for postcode");

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
                    log.LogError(e, $"Unable to get long and lat for postcode: {addressPatchRequest.PostCode}");
                    throw;
                }

                addressPatchRequest.SetLongitudeAndLatitude(position);
            }

            var doesCustomerExist = await _resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
                return ReturnNoContent(log,"Customer with given Customer Guid does not exist",customerGuid);

            var isCustomerReadOnly = await _resourceHelper.IsCustomerReadOnly(customerGuid);

            if (isCustomerReadOnly)
            {
                var forbidden = _httpResponseMessageHelper.Forbidden(customerGuid);
                log.LogWarning($"Readonly Customer. Response Code [{forbidden.StatusCode}]");
                return forbidden;                
            }    

            var address = await _addressPatchService.GetAddressForCustomerAsync(customerGuid, addressGuid);

            if (string.IsNullOrEmpty(address)) 
                return ReturnNoContent(log,"No Address record associated with the given customerId.",customerGuid);

            log.LogInformation($"Attempting to get patch customer resource {customerGuid}");

            var patchedAddress = _addressPatchService.PatchResource(address, addressPatchRequest,log);

            if (patchedAddress == null) 
                return ReturnNoContent(log,$"Related patch address not found for the address guid [{addressGuid}].",addressGuid);


            var updatedAddress = await _addressPatchService.UpdateCosmosAsync(patchedAddress, addressGuid,log);

            if (updatedAddress != null)
            {
                await _addressPatchService.SendToServiceBusQueueAsync(updatedAddress, customerGuid, ApimURL);
                var okResponse = _httpResponseMessageHelper.Ok(_jsonHelper.SerializeObjectAndRenameIdProperty(updatedAddress, "id", "AddressId"));
                log.LogInformation($"End of PATCH Addres Request. Address Update Complete and Response Status Code [{okResponse.StatusCode}]");
                return okResponse;
            }

            return ReturnBadRequest( log,"End of PATCH Addres Request. Updated Address is null.",addressGuid);
        
        }
        private HttpResponseMessage ReturnBadRequest(ILogger log,string message)
        {
             var badRequest = _httpResponseMessageHelper.BadRequest();
            log.LogWarning($"{message}. Response Code [{badRequest.StatusCode}]");
            return badRequest;
        }
        private HttpResponseMessage ReturnBadRequest(ILogger log,string message,Guid guid)
        {
             var badRequest = _httpResponseMessageHelper.BadRequest(guid);
            log.LogWarning($"{message}. Response Code [{badRequest.StatusCode}]");
            return badRequest;
        }
        private HttpResponseMessage ReturnNoContent(ILogger log,string message,Guid guid)
        {
             var noContent = _httpResponseMessageHelper.NoContent(guid);
            log.LogWarning($"{message}. Response Code [{noContent.StatusCode}]");
            return noContent;
        }
    }
}