using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http.Description;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using NCS.DSS.Address.Annotations;
using NCS.DSS.Address.Cosmos.Helper;
using NCS.DSS.Address.PatchAddressHttpTrigger.Service;
using NCS.DSS.Address.Validation;
using Newtonsoft.Json;

namespace NCS.DSS.Address.PatchAddressHttpTrigger.Function
{
    public static class PatchAddressHttpTrigger
    {
        [FunctionName("Patch")]
        [ResponseType(typeof(Models.AddressPatch))]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Address Updated", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Address does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = 422, Description = "Address validation error(s)", ShowSchema = false)]
        [Display(Name = "Patch", Description = "Ability to update an existing address.")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "Customers/{customerId}/Addresses/{addressId}")]HttpRequestMessage req, TraceWriter log, string customerId, string addressId)
        {
            log.Info("C# HTTP trigger function processed a request.");

            if (!Guid.TryParse(customerId, out var customerGuid) || 
                !Guid.TryParse(addressId, out var addressGuid))
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(addressId),
                        System.Text.Encoding.UTF8, "application/json")
                };
            }

            // Get request body
            var addressPatch = await req.Content.ReadAsAsync<Models.AddressPatch>();

            // validate the request
            var validate = new Validate();
            var errors = validate.ValidateResource(addressPatch);

            if (errors != null && errors.Any())
            {
                return new HttpResponseMessage((HttpStatusCode)422)
                {
                    Content = new StringContent("Validation error(s) : " +
                                                JsonConvert.SerializeObject(errors),
                        System.Text.Encoding.UTF8, "application/json")
                };
            }

            var resourceHelper = new ResourceHelper();
            var doesCustomerExist = resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
            {
                return new HttpResponseMessage(HttpStatusCode.NoContent)
                {
                    Content = new StringContent("Unable to find a customer with Id of : " +
                                                JsonConvert.SerializeObject(customerGuid),
                        System.Text.Encoding.UTF8, "application/json")
                };
            }
            
            var addressPatchService = new PatchAddressHttpTriggerService();
            var address = await addressPatchService.GetAddressAsync(addressGuid);

            if (address == null)
            {
                return new HttpResponseMessage(HttpStatusCode.NoContent)
                {
                    Content = new StringContent("Unable to find a address with Id of : " +
                                                JsonConvert.SerializeObject(errors),
                        System.Text.Encoding.UTF8, "application/json")
                };
            }

           var updatedAddress = await addressPatchService.UpdateAsync(address, addressPatch);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(updatedAddress),
                    System.Text.Encoding.UTF8, "application/json")
            };
        }
    }
}