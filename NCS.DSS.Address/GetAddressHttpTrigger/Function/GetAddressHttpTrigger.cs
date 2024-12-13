using DFC.HTTP.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.Address.Cosmos.Helper;
using NCS.DSS.Address.GetAddressHttpTrigger.Service;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;

namespace NCS.DSS.Address.GetAddressHttpTrigger.Function
{
    public class GetAddressHttpTrigger
    {
        private readonly IResourceHelper _resourceHelper;
        private readonly IGetAddressHttpTriggerService _getAddressService;
        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly ILogger<GetAddressHttpTrigger> _logger;

        public GetAddressHttpTrigger(
            IResourceHelper resourceHelper,
            IGetAddressHttpTriggerService getAddressService,
            IHttpRequestHelper httpRequestHelper,
            ILogger<GetAddressHttpTrigger> logger
            )
        {
            _resourceHelper = resourceHelper;
            _getAddressService = getAddressService;
            _httpRequestHelper = httpRequestHelper;
            _logger = logger;
        }

        [Function("Get")]
        [ProducesResponseType(typeof(Models.Address), 200)]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Addresses found", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Addresses do not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Display(Name = "Get", Description = "Ability to retrieve all addresses for a given customer.")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Customers/{customerId}/Addresses")] HttpRequest req, string customerId)
        {
            _logger.LogInformation("Function {FunctionName} has been invoked", nameof(GetAddressHttpTrigger));

            var touchpointId = _httpRequestHelper.GetDssTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                _logger.LogWarning("Unable to locate 'TouchpointId' in request header");
                return new BadRequestObjectResult(HttpStatusCode.BadRequest);
            }

            if (!Guid.TryParse(customerId, out var customerGuid))
            {
                _logger.LogWarning("Unable to parse 'customerId' to a GUID. Customer GUID: {CustomerID}", customerId);
                return new BadRequestObjectResult(customerGuid);
            }

            _logger.LogInformation("Input validation has succeeded. Touchpoint ID: {TouchpointId}.", touchpointId);

            _logger.LogInformation("Attempting to check if customer exists. Customer GUID: {CustomerId}", customerGuid);
            var doesCustomerExist = await _resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
            {
                _logger.LogWarning("Customer does not exist. Customer GUID: {CustomerGuid}.", customerGuid);
                return new NoContentResult();
            }
            _logger.LogInformation("Customer exists. Customer GUID: {CustomerGuid}.", customerGuid);

            _logger.LogInformation("Attempting to get Addresses for Customer. Customer GUID: {CustomerId}.", customerGuid);
            var addresses = await _getAddressService.GetAddressesAsync(customerGuid);

            if (addresses == null || addresses.Count == 0)
            {
                _logger.LogWarning("No Address found for Customer with ID: {CustomerId}.", customerGuid);
                _logger.LogInformation("Function {FunctionName} has finished invoking", nameof(GetAddressHttpTrigger));
                return new NoContentResult();
            }

            if (addresses.Count == 1)
            {
                _logger.LogWarning("1 Address found for Customer with ID: {CustomerId}.", customerGuid);
                _logger.LogInformation("Function {FunctionName} has finished invoking", nameof(GetAddressHttpTrigger));
                return new JsonResult(addresses[0], new JsonSerializerOptions())
                {
                    StatusCode = (int)HttpStatusCode.OK
                };
            }

            _logger.LogInformation("{Count} Address(es) retrieved for Customer GUID: {CustomerId}.", addresses.Count, customerGuid);
            _logger.LogInformation("Function {FunctionName} has finished invoking", nameof(GetAddressHttpTrigger));
            return new JsonResult(addresses, new JsonSerializerOptions())
            {
                StatusCode = (int)HttpStatusCode.OK
            };
        }
    }
}