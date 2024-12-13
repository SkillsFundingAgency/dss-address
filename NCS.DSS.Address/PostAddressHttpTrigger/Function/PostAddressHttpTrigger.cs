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
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;

namespace NCS.DSS.Address.PostAddressHttpTrigger.Function
{
    public class PostAddressHttpTrigger
    {
        private readonly IResourceHelper _resourceHelper;
        private readonly IValidate _validate;
        private readonly IPostAddressHttpTriggerService _addressPostService;
        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly IGeoCodingService _geoCodingService;
        private readonly ILogger<PostAddressHttpTrigger> _logger;
        private readonly IDynamicHelper _dynamicHelper;

        public PostAddressHttpTrigger(IResourceHelper resourceHelper,
            IValidate validate,
            IPostAddressHttpTriggerService addressPostService,
            IHttpRequestHelper httpRequestHelper,
            IGeoCodingService geoCodingService,
            ILogger<PostAddressHttpTrigger> logger,
            IDynamicHelper dynamicHelper)
        {
            _resourceHelper = resourceHelper;
            _validate = validate;
            _addressPostService = addressPostService;
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
            _logger.LogInformation("Function {FunctionName} has been invoked", nameof(PostAddressHttpTrigger));

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

            _logger.LogInformation("Input validation has succeeded. Touchpoint ID: {TouchpointId}.", touchpointId);

            Models.Address addressRequest;

            try
            {
                _logger.LogInformation("Attempting to retrieve resource from request. Correlation GUID: {CorrelationGuid}", correlationGuid);
                addressRequest = await _httpRequestHelper.GetResourceFromRequest<Models.Address>(req);
            }
            catch (Newtonsoft.Json.JsonException ex)
            {                
                _logger.LogError(ex, "Unable to parse {addressRequest} from request body. Correlation GUID: {CorrelationGuid}. Exception: {ExceptionMessage}", nameof(addressRequest), correlationGuid, ex.Message);
                return new UnprocessableEntityObjectResult(_dynamicHelper.ExcludeProperty(ex, ["TargetSite"]));
            }

            if (addressRequest == null)
            {                
                _logger.LogWarning("{addressRequest} object is NULL. Correlation GUID: {CorrelationGuid}", nameof(addressRequest), correlationGuid);
                return new UnprocessableEntityObjectResult(req);
            }

            _logger.LogInformation("Attempting to set IDs for Address PATCH. Correlation GUID: {CorrelationGuid}", correlationGuid);
            addressRequest.SetIds(customerGuid, touchpointId, subcontractorId);
            _logger.LogInformation("IDs successfully set for Address PATCH. Correlation GUID: {CorrelationGuid}", correlationGuid);

            try
            {
                addressRequest.PostCode = addressRequest?.PostCode?.TrimEnd().TrimStart();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to trim the postcode: {PostCode}. Exception: {ExceptionMessage}", addressRequest.PostCode, ex.Message);
                throw;
            }

            _logger.LogInformation("Attempting to validate {addressRequest} object", nameof(addressRequest));
            var errors = _validate.ValidateResource(addressRequest, true);

            if (errors != null && errors.Any())
            {
                _logger.LogWarning("Falied to validate {addressRequest} object", nameof(addressRequest));
                return new UnprocessableEntityObjectResult(errors);
            }
            _logger.LogInformation("Successfully validated {addressRequest} object", nameof(addressRequest));

            
            Position position;
            try
            {
                _logger.LogInformation("Attempting to get long and lat for postcode: {Postcode}", addressRequest.PostCode);
                position = await _geoCodingService.GetPositionForPostcodeAsync(addressRequest.PostCode);
            }
            catch (Exception ex)
            {                
                _logger.LogError(ex, "Unable to get long and lat for postcode: {PostCode}. Exception: {ExceptionMessage}", addressRequest.PostCode, ex.Message);
                throw;
            }

            addressRequest.SetLongitudeAndLatitude(position);
            _logger.LogInformation("Successfully set long and lat for postcode: {Postcode}", addressRequest.PostCode);

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

            _logger.LogInformation("Attempting to create Address in Cosmos DB. Address GUID: {AddressId}", addressRequest.AddressId);
            var address = await _addressPostService.CreateAsync(addressRequest, _logger);

            if (address == null)
            {
                _logger.LogWarning("Failed to create Address in Cosmos DB. Address GUID: {AddressId}", addressRequest.AddressId);
                _logger.LogInformation("Function {FunctionName} has finished invoking", nameof(PostAddressHttpTrigger));
                return new BadRequestObjectResult(customerGuid);
            }

            _logger.LogInformation("Address created successfully in Cosmos DB. Address GUID: {AddressId}", address.AddressId);


            _logger.LogInformation("Attempting to send message to Service Bus Namespace. Address GUID: {AddressId}", address.AddressId);
            await _addressPostService.SendToServiceBusQueueAsync(address, apimUrl);
            _logger.LogInformation("Successfully sent message to Service Bus. Address GUID: {AddressId}", address.AddressId);


            _logger.LogInformation("Function {FunctionName} has finished invoking", nameof(PostAddressHttpTrigger));

            return new JsonResult(address, new JsonSerializerOptions())
            {
                StatusCode = (int)HttpStatusCode.Created
            };
        }       
    }
}