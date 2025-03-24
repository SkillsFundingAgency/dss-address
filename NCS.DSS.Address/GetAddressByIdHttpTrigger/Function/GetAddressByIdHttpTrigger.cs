using DFC.HTTP.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.Address.Cosmos.Helper;
using NCS.DSS.Address.GetAddressByIdHttpTrigger.Service;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;

namespace NCS.DSS.Address.GetAddressByIdHttpTrigger.Function
{
    public class GetAddressByIdHttpTrigger
    {
        private readonly IResourceHelper _resourceHelper;
        private readonly IGetAddressByIdHttpTriggerService _getAddressByIdService;
        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly ILogger<GetAddressByIdHttpTrigger> _logger;

        public GetAddressByIdHttpTrigger(
            IResourceHelper resourceHelper,
            IGetAddressByIdHttpTriggerService getAddressByIdService,
            IHttpRequestHelper httpRequestHelper,
            ILogger<GetAddressByIdHttpTrigger> logger)
        {
            _resourceHelper = resourceHelper;
            _getAddressByIdService = getAddressByIdService;
            _httpRequestHelper = httpRequestHelper;
            _logger = logger;
        }

        [Function("GetById")]
        [ProducesResponseType(typeof(Models.Address), 200)]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Address found", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Address does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Display(Name = "Get", Description = "Ability to retrieve a single address with a given AddressId for an individual customer.")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Customers/{customerId}/Addresses/{addressId}")] HttpRequest req, string customerId, string addressId)
        {
            _logger.LogInformation("Function {FunctionName} has been invoked", nameof(GetAddressByIdHttpTrigger));

            var touchpointId = _httpRequestHelper.GetDssTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                _logger.LogWarning("Unable to locate 'TouchpointId' in request header");
                return new BadRequestObjectResult(HttpStatusCode.BadRequest);
            }

            if (!Guid.TryParse(customerId, out var customerGuid))
            {
                _logger.LogWarning("Unable to parse 'customerId' to a GUID. Customer GUID: {CustomerID}", customerId);
                return new BadRequestObjectResult(customerId);
            }

            if (!Guid.TryParse(addressId, out var addressGuid))
            {
                _logger.LogWarning("Unable to parse 'addressId' to a GUID. Address GUID: {AddressID}", addressId);
                return new BadRequestObjectResult(addressId);
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

            _logger.LogInformation("Attempting to get Address for Customer. Customer GUID: {CustomerId}. Address GUID: {AddressId}.", customerGuid, addressGuid);
            var address = await _getAddressByIdService.GetAddressForCustomerAsync(customerGuid, addressGuid);

            if (address == null)
            {
                _logger.LogWarning("Address not found. Customer GUID: {CustomerId}. Address GUID: {AddressId}.", customerGuid, addressGuid);
                _logger.LogInformation("Function {FunctionName} has finished invoking", nameof(GetAddressByIdHttpTrigger));
                return new NoContentResult();
            }

            _logger.LogInformation("Address successfully retrieved. Address GUID: {AddressId}", address.AddressId);
            _logger.LogInformation("Function {FunctionName} has finished invoking", nameof(GetAddressByIdHttpTrigger));

            return new JsonResult(address, new JsonSerializerOptions())
            {
                StatusCode = (int)HttpStatusCode.OK
            };
        }
    }
}