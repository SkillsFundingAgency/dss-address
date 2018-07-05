using System.Net.Http;
using System.Threading.Tasks;

namespace NCS.DSS.Address.Helpers
{
    public class HttpRequestMessageHelper : IHttpRequestMessageHelper
    {
        public async Task<T> GetDiversityFromRequest<T>(HttpRequestMessage req)
        {
            return await req.Content.ReadAsAsync<T>();
        }
    }
}
