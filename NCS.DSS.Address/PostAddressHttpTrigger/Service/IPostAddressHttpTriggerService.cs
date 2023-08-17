using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace NCS.DSS.Address.PostAddressHttpTrigger.Service
{
    public interface IPostAddressHttpTriggerService
    {
        Task<Models.Address> CreateAsync(Models.Address address, ILogger logger);
        Task SendToServiceBusQueueAsync(Models.Address address, string reqUrl);
    }
}