using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http.Description;
using NCS.DSS.Address.Annotations;

namespace NCS.DSS.Address.GetAddressByIdHttpTrigger
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
        [Display(Name = "Get", Description = "Ability to retrieve a single address with a given Id for an individual customer.")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Customers/{customerId}/Addresses/{addressId}")]HttpRequestMessage req, TraceWriter log, string customerId, string addressId)
        {
            log.Info("C# HTTP trigger function processed a request.");

            if (!Guid.TryParse(addressId, out var addressGuid))
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(addressId),
                        System.Text.Encoding.UTF8, "application/json")
                };
            }
            var service = new GetAddressByIdHttpTriggerService();
            var values = await service.GetAddress(addressGuid);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(values),
                    System.Text.Encoding.UTF8, "application/json")
            };
        }
    }
}