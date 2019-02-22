using System;
using System.Net;
using System.Threading.Tasks;
using NCS.DSS.Address.Cosmos.Provider;
using NCS.DSS.Address.Models;
using NCS.DSS.Address.ServiceBus;

namespace NCS.DSS.Address.PatchAddressHttpTrigger.Service
{
    public class PatchAddressHttpTriggerService : IPatchAddressHttpTriggerService
    {

        private readonly IDocumentDBProvider _documentDbProvider;
        private readonly IAddressPatchService _addressPatchService;

        public PatchAddressHttpTriggerService(IDocumentDBProvider documentDbProvider, IAddressPatchService addressPatchService)
        {
            _documentDbProvider = documentDbProvider;
            _addressPatchService = addressPatchService;
        }

        public string PatchResource(string addressJson, AddressPatch addressPatch)
        {
            if (string.IsNullOrEmpty(addressJson))
                return null;

            if (addressPatch == null)
                return null;

            addressPatch.SetDefaultValues();

            return _addressPatchService.Patch(addressJson, addressPatch);
        }

        public async Task<Models.Address> UpdateCosmosAsync(string addressJson, Guid addressId)
        {
            var response = await _documentDbProvider.UpdateAddressAsync(addressJson, addressId);

            var responseStatusCode = response?.StatusCode;

            return responseStatusCode == HttpStatusCode.OK ? (dynamic)response.Resource : null;
        }

        public async Task<string> GetAddressForCustomerAsync(Guid customerId, Guid addressId)
        {
            return await _documentDbProvider.GetAddressByIdForUpdateAsync(customerId, addressId);
        }

        public async Task SendToServiceBusQueueAsync(Models.Address address, Guid customerId, string reqUrl)
        {
            await ServiceBusClient.SendPatchMessageAsync(address, customerId, reqUrl);
        }
    }
}