using System;
using System.Threading.Tasks;
using NCS.DSS.Address.Models;

namespace NCS.DSS.Address.PatchAddressHttpTrigger.Service
{
    public interface IPatchAddressHttpTriggerService
    {
        Models.Address PatchResource(string addressJson, AddressPatch addressPatch);
        Task<Models.Address> UpdateCosmosAsync(Models.Address address, Models.AddressPatch addressPatch);
        Task<string> GetAddressForCustomerAsync(Guid customerId, Guid addressId);
        Task SendToServiceBusQueueAsync(Models.Address address, Guid customerId, string reqUrl);
    }
}