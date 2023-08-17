using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NCS.DSS.Address.Models;

namespace NCS.DSS.Address.PatchAddressHttpTrigger.Service
{
    public interface IPatchAddressHttpTriggerService
    {
        string PatchResource(string addressJson, AddressPatch addressPatch, ILogger logger);
        Task<Models.Address> UpdateCosmosAsync(string addressJson, Guid addressId, ILogger logger);
        Task<string> GetAddressForCustomerAsync(Guid customerId, Guid addressId);
        Task SendToServiceBusQueueAsync(Models.Address address, Guid customerId, string reqUrl);
    }
}