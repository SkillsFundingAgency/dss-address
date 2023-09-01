using DFC.Common.Standard.Logging;
using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using NCS.DSS.Address.Cosmos.Helper;
using NCS.DSS.Address.GetAddressHttpTrigger.Service;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace NCS.DSS.Address.GetAddressHttpTrigger.Function
{
    public class GetAddressHttpTrigger
    {
        private IResourceHelper _resourceHelper;
        private IGetAddressHttpTriggerService _getAddressService;
        private ILoggerHelper _loggerHelper;
        private IHttpRequestHelper _httpRequestHelper;
        private IHttpResponseMessageHelper _httpResponseMessageHelper;
        private IJsonHelper _jsonHelper;

        public GetAddressHttpTrigger(
            IResourceHelper resourceHelper,
            IGetAddressHttpTriggerService getAddressService,
            ILoggerHelper loggerHelper,
            IHttpRequestHelper httpRequestHelper,
            IHttpResponseMessageHelper httpResponseMessageHelper,
            IJsonHelper jsonHelper
            )
        {
            _resourceHelper = resourceHelper;
            _getAddressService = getAddressService;
            _loggerHelper = loggerHelper;
            _httpRequestHelper = httpRequestHelper;
            _httpResponseMessageHelper = httpResponseMessageHelper;
            _jsonHelper = jsonHelper;
        }

        [FunctionName("Get")]
        [ProducesResponseType(typeof(Models.Address), 200)]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Addresses found", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Addresses do not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Display(Name = "Get", Description = "Ability to retrieve all addresses for a given customer.")]
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Customers/{customerId}/Addresses")] HttpRequest req, ILogger log, string customerId)
        {
            log.LogInformation(string.Format("Start getting address by the CustomerId [{0}]",customerId));
            
            var touchpointId = _httpRequestHelper.GetDssTouchpointId(req);
            
            if (string.IsNullOrEmpty(touchpointId))
                return ReturnBadRequest( log,"Unable to locate 'TouchpointId' in request header.");

            log.LogInformation("Get Address C# HTTP trigger function  processed a request. By Touchpoint " + touchpointId);

            if (!Guid.TryParse(customerId, out var customerGuid))
                return ReturnBadRequest(log,$"Failed to parse customerId to Guid [{customerId}].",customerGuid);                

            var doesCustomerExist = await _resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
                return ReturnNoContent(log,$"Customer with given Customer Guid does not exist.",customerGuid); 

            var addresses = await _getAddressService.GetAddressesAsync(customerGuid);

            if(addresses != null){
                var okResponse = _httpResponseMessageHelper.Ok(_jsonHelper.SerializeObjectsAndRenameIdProperty(addresses, "id", "AddressId"));
                log.LogInformation($"{addresses.Count} Addresses found. Returning Addresses in Json format as a Response. Response Code [{okResponse.StatusCode}]");
                return okResponse;
            }

            return ReturnNoContent(log,$"No Addresses Found on a given customer id.",customerGuid); 
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