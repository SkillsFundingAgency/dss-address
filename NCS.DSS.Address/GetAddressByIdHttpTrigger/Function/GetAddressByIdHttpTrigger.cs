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
using NCS.DSS.Address.GetAddressByIdHttpTrigger.Service;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace NCS.DSS.Address.GetAddressByIdHttpTrigger.Function
{
    public class GetAddressByIdHttpTrigger
    {
        private IResourceHelper _resourceHelper;
        private IGetAddressByIdHttpTriggerService _getAddressByIdService;
        private ILoggerHelper _loggerHelper;
        private IHttpRequestHelper _httpRequestHelper;
        private IHttpResponseMessageHelper _httpResponseMessageHelper;
        private IJsonHelper _jsonHelper;

        public GetAddressByIdHttpTrigger(
            IResourceHelper resourceHelper,
            IGetAddressByIdHttpTriggerService getAddressByIdService,
            ILoggerHelper loggerHelper,
            IHttpRequestHelper httpRequestHelper,
            IHttpResponseMessageHelper httpResponseMessageHelper,
            IJsonHelper jsonHelper)
        {
            _resourceHelper = resourceHelper;
            _getAddressByIdService = getAddressByIdService;
            _loggerHelper = loggerHelper;
            _httpRequestHelper = httpRequestHelper;
            _httpResponseMessageHelper = httpResponseMessageHelper;
            _jsonHelper = jsonHelper;
        }

        [FunctionName("GetById")]
        [ProducesResponseType(typeof(Models.Address), 200)]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Address found", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Address does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Display(Name = "Get", Description = "Ability to retrieve a single address with a given AddressId for an individual customer.")]
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Customers/{customerId}/Addresses/{addressId}")]HttpRequest req, ILogger log, string customerId, string addressId)
        {
            var touchpointId = _httpRequestHelper.GetDssTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                log.LogInformation("Unable to locate 'TouchpointId' in request header");
                return _httpResponseMessageHelper.BadRequest();
            }

            var subcontractorId = _httpRequestHelper.GetDssSubcontractorId(req);
            if (string.IsNullOrEmpty(subcontractorId))
            {
                log.LogInformation("Unable to locate 'subcontractorId' in request header");
                return _httpResponseMessageHelper.BadRequest();
            }

            log.LogInformation("Get Address By Id C# HTTP trigger function  processed a request. By Touchpoint " + touchpointId);

            if (!Guid.TryParse(customerId, out var customerGuid))
                return _httpResponseMessageHelper.BadRequest(customerGuid);

            if (!Guid.TryParse(addressId, out var addressGuid))
                return _httpResponseMessageHelper.BadRequest(addressGuid);

            var doesCustomerExist = await _resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
                return _httpResponseMessageHelper.NoContent(customerGuid);

            var address = await _getAddressByIdService.GetAddressForCustomerAsync(customerGuid, addressGuid);

            return address == null ?
                _httpResponseMessageHelper.NoContent(addressGuid) :
                _httpResponseMessageHelper.Ok(_jsonHelper.SerializeObjectAndRenameIdProperty(address, "id", "AddressId"));
        }
    }
}