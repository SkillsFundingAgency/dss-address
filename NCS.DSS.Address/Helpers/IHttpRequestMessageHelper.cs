using System.Net.Http;
using System.Threading.Tasks;

namespace NCS.DSS.Address.Helpers
{
    public interface IHttpRequestMessageHelper
    {
        Task<T> GetDiversityFromRequest<T>(HttpRequestMessage req);
    }
}