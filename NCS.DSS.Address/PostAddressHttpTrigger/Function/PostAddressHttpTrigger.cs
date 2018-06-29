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
using NCS.DSS.Address.PostAddressHttpTrigger.Service;
using NCS.DSS.Address.Validation;
using Newtonsoft.Json;

namespace NCS.DSS.Address.PostAddressHttpTrigger.Function
{
    public static class PostAddressHttpTrigger
    {
        [FunctionName("Post")]
        [ResponseType(typeof(Models.Address))]
        [Response(HttpStatusCode = (int)HttpStatusCode.Created, Description = "Address Created", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Address does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = 422, Description = "Address validation error(s)", ShowSchema = false)]
        [Display(Name = "Post", Description = "Ability to create a new address for a given customer")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Customers/{customerId}/Addresses")]HttpRequestMessage req, TraceWriter log, string customerId)
        {
            log.Info("C# HTTP trigger function processed a request.");

            // Get request body
            var address = await req.Content.ReadAsAsync<Models.Address>();

            // validate the request
            var validate = new Validate();
            var errors = validate.ValidateResource(address);

            if (errors != null && errors.Any())
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(errors),
                        System.Text.Encoding.UTF8, "application/json")
                };
            }

            var addressService = new PostAddressHttpTriggerService();
            var addressId = await addressService.CreateAsync(address);

            return addressId == null
                ? new HttpResponseMessage(HttpStatusCode.BadRequest)
                : new HttpResponseMessage(HttpStatusCode.Created)
                {
                    Content = new StringContent("Created Address record with Id of : " + addressId)
                };
        }
    }
}