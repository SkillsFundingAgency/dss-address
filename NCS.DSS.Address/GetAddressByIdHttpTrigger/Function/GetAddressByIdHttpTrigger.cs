using DFC.HTTP.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.Address.Cosmos.Helper;
using NCS.DSS.Address.GetAddressByIdHttpTrigger.Service;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace NCS.DSS.Address.GetAddressByIdHttpTrigger.Function
{
    public class GetAddressByIdHttpTrigger
    {
        private readonly IResourceHelper _resourceHelper;
        private readonly IGetAddressByIdHttpTriggerService _getAddressByIdService;
        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly ILogger _logger;

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
            _logger.LogInformation(string.Format("Start getting address by the AddressId [{0}] and CustomerId [{1}]", addressId, customerId));

            var touchpointId = _httpRequestHelper.GetDssTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                _logger.LogInformation("Unable to locate 'TouchpointId' in request header");
                return new BadRequestObjectResult(HttpStatusCode.BadRequest);
            }

            _logger.LogInformation("Get Address By Id C# HTTP trigger function  processed a request. By Touchpoint " + touchpointId);

            if (!Guid.TryParse(customerId, out var customerGuid))
                return new BadRequestObjectResult(customerGuid);

            if (!Guid.TryParse(addressId, out var addressGuid))
                return new BadRequestObjectResult(addressGuid);

            var doesCustomerExist = await _resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
                return new NoContentResult();

            var address = await _getAddressByIdService.GetAddressForCustomerAsync(customerGuid, addressGuid);

            if (address == null)
            {
                _logger.LogInformation("Address not found. Returning NO CONTENT Response");
                return new NoContentResult();
            }

            _logger.LogInformation("Address found. Returning Address in Json format as a Response");

            return new JsonResult(address, new JsonSerializerOptions())
            {
                StatusCode = (int)HttpStatusCode.OK
            };
        }
    }
}