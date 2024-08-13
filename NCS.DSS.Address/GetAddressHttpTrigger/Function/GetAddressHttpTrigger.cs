using DFC.HTTP.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.Address.Cosmos.Helper;
using NCS.DSS.Address.GetAddressHttpTrigger.Service;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace NCS.DSS.Address.GetAddressHttpTrigger.Function
{
    public class GetAddressHttpTrigger
    {
        private readonly IResourceHelper _resourceHelper;
        private readonly IGetAddressHttpTriggerService _getAddressService;
        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly ILogger _logger;

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
            _logger.LogInformation(string.Format("Start getting address by the CustomerId [{0}]", customerId));

            var touchpointId = _httpRequestHelper.GetDssTouchpointId(req);

            if (string.IsNullOrEmpty(touchpointId))
                return ReturnBadRequest("Unable to locate 'TouchpointId' in request header.");

            _logger.LogInformation("Get Address C# HTTP trigger function  processed a request. By Touchpoint " + touchpointId);

            if (!Guid.TryParse(customerId, out var customerGuid))
                return ReturnBadRequest($"Failed to parse customerId to Guid [{customerId}].", customerGuid);

            var doesCustomerExist = await _resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
                return ReturnNoContent($"Customer with given Customer Guid does not exist.", customerGuid);

            var addresses = await _getAddressService.GetAddressesAsync(customerGuid);

            if (addresses != null)
            {
                _logger.LogInformation($"{addresses.Count} Addresses found. Returning Addresses in Json format as a Response. Response Code [{HttpStatusCode.OK}]");
                return new JsonResult(addresses, new JsonSerializerOptions())
                {
                    StatusCode = (int)HttpStatusCode.OK
                };
            }

            return ReturnNoContent($"No Addresses Found on a given customer id.", customerGuid);
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