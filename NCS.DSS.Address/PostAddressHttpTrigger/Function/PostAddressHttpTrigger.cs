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
using NCS.DSS.Address.PostAddressHttpTrigger.Service;
using NCS.DSS.Address.Validation;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace NCS.DSS.Address.PostAddressHttpTrigger.Function
{
    public class PostAddressHttpTrigger
    {
        private IResourceHelper _resourceHelper;
        private IValidate _validate;
        private IPostAddressHttpTriggerService _addressPostService;
        private ILoggerHelper _loggerHelper;
        private IHttpRequestHelper _httpRequestHelper;
        private IHttpResponseMessageHelper _httpResponseMessageHelper;
        private IJsonHelper _jsonHelper;
        private IGeoCodingService _geoCodingService;

        public PostAddressHttpTrigger(IResourceHelper resourceHelper,
            IValidate validate,
            IPostAddressHttpTriggerService addressPostService,
            ILoggerHelper loggerHelper,
            IHttpRequestHelper httpRequestHelper,
            IHttpResponseMessageHelper httpResponseMessageHelper,
            IJsonHelper jsonHelper,
            IGeoCodingService geoCodingService)
        {
            _resourceHelper = resourceHelper;
            _validate = validate;
            _addressPostService = addressPostService;
            _loggerHelper = loggerHelper;
            _httpRequestHelper = httpRequestHelper;
            _httpResponseMessageHelper = httpResponseMessageHelper;
            _jsonHelper = jsonHelper;
            _geoCodingService = geoCodingService;
        }

        [FunctionName("Post")]
        [ProducesResponseType(typeof(Models.Address), 201)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Created, Description = "Address Created", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Address does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = 422, Description = "Address validation error(s)", ShowSchema = false)]
        [Display(Name = "Post", Description = "Ability to create a new address for a given customer")]
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Customers/{customerId}/Addresses")]HttpRequest req, ILogger log, string customerId)
        {
            log.LogInformation($"Started Executing Address POST Request for CustomerId [{customerId}]");

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
                log.LogInformation( "Unable to locate 'SubContractorId' in request header. Continuing POST Process");

            log.LogInformation("Post Address C# HTTP trigger function  processed a request. By Touchpoint " + touchpointId);

            if (!Guid.TryParse(customerId, out var customerGuid))
                return ReturnBadRequest( log,"Unable to Parse customerId to Guid",customerGuid);

            Models.Address addressRequest;

            try
            {
                addressRequest = await _httpRequestHelper.GetResourceFromRequest<Models.Address>(req);
            }
            catch (JsonException ex)
            {
                var unProcessed = _httpResponseMessageHelper.UnprocessableEntity(ex);
                log.LogWarning($"Failed to Prase Json object. Response Code [{unProcessed.StatusCode}]. Exception Message: [{ex.Message}]");
                return unProcessed;
            }

            if (addressRequest == null)
            {
                var unProcessed = _httpResponseMessageHelper.UnprocessableEntity(req);
                log.LogWarning($"Address Request is Empty. Response Code [{unProcessed.StatusCode}]");
                return unProcessed;
            }    

            addressRequest.SetIds(customerGuid, touchpointId, subContractorId);

            var errors = _validate.ValidateResource(addressRequest, true);

            if (errors != null && errors.Any())
            {
                var unProcessed = _httpResponseMessageHelper.UnprocessableEntity(errors);
                log.LogWarning($"Validation errors occured while processing request. Response Code [{unProcessed.StatusCode}]");
                log.LogWarning($"Validation Failures are [{string.Join(',',errors)}]");
                return unProcessed;
            }
                
            _loggerHelper.LogInformationMessage(log, correlationGuid, "Attempting to get long and lat for postcode");
            Position position;

            try
            {
                var postcode = addressRequest.PostCode.Replace(" ", string.Empty);
                position = await _geoCodingService.GetPositionForPostcodeAsync(postcode);
            }
            catch (Exception e)
            {
                _loggerHelper.LogException(log, correlationGuid, string.Format("Unable to get long and lat for postcode: {0}", addressRequest.PostCode), e);
                throw;
            }

            addressRequest.SetLongitudeAndLatitude(position);

            var doesCustomerExist = await _resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
                return ReturnNoContent(log,"Customer with given Customer Guid does not exist",customerGuid);              

            var isCustomerReadOnly = await _resourceHelper.IsCustomerReadOnly(customerGuid);

            if (isCustomerReadOnly){
                var forbidden = _httpResponseMessageHelper.Forbidden(customerGuid);
                log.LogWarning($"Readonly Customer. Response Code [{forbidden.StatusCode}]");
                return forbidden;                
            }               

            var address = await _addressPostService.CreateAsync(addressRequest,log);

            if (address != null)
                await _addressPostService.SendToServiceBusQueueAsync(address, ApimURL);

            _loggerHelper.LogMethodExit(log);

            if(address == null)            
                return ReturnBadRequest( log,"Null Address Found",customerGuid);
            
            var created =  _httpResponseMessageHelper.Created(_jsonHelper.SerializeObjectAndRenameIdProperty(address, "id", "AddressId"));
            log.LogInformation($"Address Found. Response Code [{created.StatusCode}]");
            return created;
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