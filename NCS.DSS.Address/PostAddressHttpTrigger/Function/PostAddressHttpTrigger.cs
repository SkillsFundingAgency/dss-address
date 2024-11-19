using DFC.Common.Standard.Logging;
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
using NCS.DSS.Address.PostAddressHttpTrigger.Service;
using NCS.DSS.Address.Validation;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace NCS.DSS.Address.PostAddressHttpTrigger.Function
{
    public class PostAddressHttpTrigger
    {
        private readonly IResourceHelper _resourceHelper;
        private readonly IValidate _validate;
        private readonly IPostAddressHttpTriggerService _addressPostService;
        private readonly ILoggerHelper _loggerHelper;
        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly IGeoCodingService _geoCodingService;
        private readonly ILogger _logger;
        private readonly IDynamicHelper _dynamicHelper;

        public PostAddressHttpTrigger(IResourceHelper resourceHelper,
            IValidate validate,
            IPostAddressHttpTriggerService addressPostService,
            ILoggerHelper loggerHelper,
            IHttpRequestHelper httpRequestHelper,
            IGeoCodingService geoCodingService,
            ILogger<PostAddressHttpTrigger> logger,
            IDynamicHelper dynamicHelper)
        {
            _resourceHelper = resourceHelper;
            _validate = validate;
            _addressPostService = addressPostService;
            _loggerHelper = loggerHelper;
            _httpRequestHelper = httpRequestHelper;
            _geoCodingService = geoCodingService;
            _logger = logger;
            _dynamicHelper = dynamicHelper;
        }

        [Function("Post")]
        [ProducesResponseType(typeof(Models.Address), 201)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Created, Description = "Address Created", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Address does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = 422, Description = "Address validation error(s)", ShowSchema = false)]
        [Display(Name = "Post", Description = "Ability to create a new address for a given customer")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Customers/{customerId}/Addresses")] HttpRequest req, string customerId)
        {
            _logger.LogInformation($"Started Executing Address POST Request for CustomerId [{customerId}]");

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
                _logger.LogInformation("Unable to locate 'SubContractorId' in request header. Continuing POST Process");

            _logger.LogInformation("Post Address C# HTTP trigger function  processed a request. By Touchpoint " + touchpointId);

            if (!Guid.TryParse(customerId, out var customerGuid))
                return ReturnBadRequest("Unable to Parse customerId to Guid", customerGuid);

            Models.Address addressRequest;

            try
            {
                addressRequest = await _httpRequestHelper.GetResourceFromRequest<Models.Address>(req);
            }
            catch (Newtonsoft.Json.JsonException ex)
            {
                _logger.LogWarning($"Failed to Prase Json object. Response Code [{HttpStatusCode.UnprocessableContent}]. Exception Message: [{ex.Message}]");
                return new UnprocessableEntityObjectResult(_dynamicHelper.ExcludeProperty(ex, ["TargetSite"]));
            }

            if (addressRequest == null)
            {
                _logger.LogWarning($"Address Request is Empty. Response Code [{HttpStatusCode.UnprocessableContent}]");
                return new UnprocessableEntityObjectResult(req);
            }

            addressRequest.SetIds(customerGuid, touchpointId, subContractorId);

            try
            {
                addressRequest.PostCode = addressRequest?.PostCode?.TrimEnd();
            } 
            catch (Exception e)
            {
                _loggerHelper.LogException(_logger, correlationGuid, string.Format("Unable to trim the postcode: `{0}`", addressRequest.PostCode), e);
                throw;
            }

            var errors = _validate.ValidateResource(addressRequest, true);

            if (errors != null && errors.Any())
            {
                _logger.LogWarning($"Validation errors occured while processing request. Response Code [{HttpStatusCode.UnprocessableContent}]");
                _logger.LogWarning($"Validation Failures are [{string.Join(',', errors)}]");
                return new UnprocessableEntityObjectResult(errors);
            }

            _loggerHelper.LogInformationMessage(_logger, correlationGuid, "Attempting to get long and lat for postcode");
            Position position;

            try
            {
                position = await _geoCodingService.GetPositionForPostcodeAsync(addressRequest.PostCode);
            }
            catch (Exception e)
            {
                _loggerHelper.LogException(_logger, correlationGuid, string.Format("Unable to get long and lat for postcode: {0}", addressRequest.PostCode), e);
                throw;
            }

            addressRequest.SetLongitudeAndLatitude(position);

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

            var address = await _addressPostService.CreateAsync(addressRequest, _logger);

            if (address != null)
                await _addressPostService.SendToServiceBusQueueAsync(address, ApimURL);

            _loggerHelper.LogMethodExit(_logger);

            if (address == null)
                return ReturnBadRequest("Null Address Found", customerGuid);


            _logger.LogInformation($"Address Found. Response Code [{HttpStatusCode.Created}]");

            return new JsonResult(address, new JsonSerializerOptions())
            {
                StatusCode = (int)HttpStatusCode.Created
            };
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