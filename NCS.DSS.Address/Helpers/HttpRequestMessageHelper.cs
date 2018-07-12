using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace NCS.DSS.Address.Helpers
{
    public class HttpRequestMessageHelper : IHttpRequestMessageHelper
    {
        public async Task<T> GetAddressFromRequest<T>(HttpRequestMessage req)
        {
            req.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return await req.Content.ReadAsAsync<T>();
        }
    }
}
