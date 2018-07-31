using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace NCS.DSS.Address.Helpers
{
    public interface IHttpRequestMessageHelper
    {
        Task<T> GetAddressFromRequest<T>(HttpRequestMessage req);
        Guid? GetTouchpointId(HttpRequestMessage req);
    }
}