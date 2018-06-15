using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http.Description;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using NCS.DSS.Address.Annotations;

namespace NCS.DSS.Address.PostAddressHttpTrigger
{
    public static class PostAddressHttpTrigger
    {
        [FunctionName("Post")]
        [ResponseType(typeof(Models.Address))]
        [AddressResponse(HttpStatusCode = (int)HttpStatusCode.Created, Description = "Address Created", ShowSchema = true)]
        [AddressResponse(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Unable to create Address", ShowSchema = false)]
        [AddressResponse(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Forbidden", ShowSchema = false)]
        [Display(Name = "Post", Description = "Ability to create a new address for a given customer")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Customers/{customerId}/Addresses")]HttpRequestMessage req, TraceWriter log, string customerId)
        {
            log.Info("C# HTTP trigger function processed a request.");

            // Get request body
            var address = await req.Content.ReadAsAsync<Models.Address>();

            var addressService = new PostAddressHttpTriggerService();
            var addressId = addressService.Create(address);

            return addressId == null
                ? new HttpResponseMessage(HttpStatusCode.BadRequest)
                : new HttpResponseMessage(HttpStatusCode.Created)
                {
                    Content = new StringContent("Created Address record with Id of : " + addressId)
                };
        }
    }
}