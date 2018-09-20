using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http.Description;
using Microsoft.Extensions.Logging;
using NCS.DSS.Address.Annotations;
using NCS.DSS.Address.Cosmos.Helper;
using NCS.DSS.Address.GetAddressByIdHttpTrigger.Service;
using NCS.DSS.Address.Helpers;
using NCS.DSS.Address.Ioc;

namespace NCS.DSS.Address.GetAddressByIdHttpTrigger.Function
{
    public static class GetAddressByIdHttpTrigger
    {
        [FunctionName("GetById")]
        [ResponseType(typeof(Models.Address))]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Address found", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Address does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Display(Name = "Get", Description = "Ability to retrieve a single address with a given AddressId for an individual customer.")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Customers/{customerId}/Addresses/{addressId}")]HttpRequestMessage req, ILogger log, string customerId, string addressId,
            [Inject]IResourceHelper resourceHelper,
            [Inject]IHttpRequestMessageHelper httpRequestMessageHelper,
            [Inject]IGetAddressByIdHttpTriggerService getAddressByIdService)
        {
            var touchpointId = httpRequestMessageHelper.GetTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                log.LogInformation("Unable to locate 'TouchpointId' in request header");
                return HttpResponseMessageHelper.BadRequest();
            }

            log.LogInformation("Get Address By Id C# HTTP trigger function  processed a request. By Touchpoint " + touchpointId);

            if (!Guid.TryParse(customerId, out var customerGuid))
                return HttpResponseMessageHelper.BadRequest(customerGuid);

            if (!Guid.TryParse(addressId, out var addressGuid))
                return HttpResponseMessageHelper.BadRequest(addressGuid);

            var doesCustomerExist = await resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
                return HttpResponseMessageHelper.NoContent(customerGuid);

            var address = await getAddressByIdService.GetAddressForCustomerAsync(customerGuid, addressGuid);

            return address == null ? 
                HttpResponseMessageHelper.NoContent(addressGuid) :
                HttpResponseMessageHelper.Ok(JsonHelper.SerializeObject(address));
        }
    }
}