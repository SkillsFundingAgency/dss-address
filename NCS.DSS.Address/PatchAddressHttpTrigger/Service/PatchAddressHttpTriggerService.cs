using System;
using System.Net;
using System.Threading.Tasks;
using NCS.DSS.Address.Cosmos.Provider;
using NCS.DSS.Address.ServiceBus;

namespace NCS.DSS.Address.PatchAddressHttpTrigger.Service
{
    public class PatchAddressHttpTriggerService : IPatchAddressHttpTriggerService
    {

        private readonly IDocumentDBProvider _documentDbProvider;

        public PatchAddressHttpTriggerService(IDocumentDBProvider documentDbProvider)
        {
            _documentDbProvider = documentDbProvider;
        }

        public async Task<Models.Address> UpdateAsync(Models.Address address, Models.AddressPatch addressPatch)
        {
            if (address == null)
                return null;

            addressPatch.SetDefaultValues();
            address.Patch(addressPatch);

            var response = await _documentDbProvider.UpdateAddressAsync(address);

            var responseStatusCode = response?.StatusCode;

            return responseStatusCode == HttpStatusCode.OK ? address : null;
        }

        public async Task<Models.Address> GetAddressForCustomerAsync(Guid customerId, Guid addressId)
        {
            var address = await _documentDbProvider.GetAddressForCustomerAsync(customerId, addressId);
            
            return address;
        }

        public async Task SendToServiceBusQueueAsync(Models.Address address, Guid customerId, string reqUrl)
        {
            await ServiceBusClient.SendPatchMessageAsync(address, customerId, reqUrl);
        }
    }
}