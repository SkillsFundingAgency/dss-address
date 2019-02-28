using System.Threading.Tasks;

namespace NCS.DSS.Address.PostAddressHttpTrigger.Service
{
    public interface IPostAddressHttpTriggerService
    {
        Task<Models.Address> CreateAsync(Models.Address address);
        Task SendToServiceBusQueueAsync(Models.Address address, string reqUrl);
    }
}