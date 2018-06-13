using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http.Description;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace NCS.DSS.Address.PostAddressHttpTrigger
{
    public static class PostAddressHttpTrigger
    {
        [FunctionName("Post")]
        [ResponseType(typeof(Models.Address))]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Customers/{customerId:guid}/Addresses")]HttpRequestMessage req, TraceWriter log)
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