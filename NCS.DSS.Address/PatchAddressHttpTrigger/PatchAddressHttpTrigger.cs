using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Web.Http.Description;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using NCS.DSS.Address.Annotations;
using Newtonsoft.Json;

namespace NCS.DSS.Address.PatchAddressHttpTrigger
{
    public static class PatchAddressHttpTrigger
    {
        [FunctionName("Patch")]
        [ResponseType(typeof(Models.Address))]
        [AddressResponse(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Address Updated", ShowSchema = true)]
        [AddressResponse(HttpStatusCode = (int)HttpStatusCode.NotFound, Description = "Supplied Address Id does not exist", ShowSchema = false)]
        [Display(Name = "Patch", Description = "Ability to update an existing address.")]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "Customers/{customerId}/Addresses/{addressId}")]HttpRequestMessage req, TraceWriter log, string customerId, string addressId)
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

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(addressGuid),
                    System.Text.Encoding.UTF8, "application/json")
            };
        }
    }
}