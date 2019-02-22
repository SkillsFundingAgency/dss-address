using System;
using System.Threading.Tasks;
using NCS.DSS.Address.Models;

namespace NCS.DSS.Address.PatchAddressHttpTrigger.Service
{
    public interface IPatchAddressHttpTriggerService
    {
        string PatchResource(string addressJson, AddressPatch addressPatch);
        Task<Models.Address> UpdateCosmosAsync(string addressJson, Guid addressId);
        Task<string> GetAddressForCustomerAsync(Guid customerId, Guid addressId);
        Task SendToServiceBusQueueAsync(Models.Address address, Guid customerId, string reqUrl);
    }
}