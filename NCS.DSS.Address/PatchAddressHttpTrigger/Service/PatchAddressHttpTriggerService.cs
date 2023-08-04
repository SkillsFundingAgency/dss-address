using System;
using System.Net;
using System.Threading.Tasks;
using NCS.DSS.Address.Cosmos.Provider;
using NCS.DSS.Address.Models;
using NCS.DSS.Address.ServiceBus;
using Microsoft.Extensions.Logging;

namespace NCS.DSS.Address.PatchAddressHttpTrigger.Service
{
    public class PatchAddressHttpTriggerService : IPatchAddressHttpTriggerService
    {

        private readonly IDocumentDBProvider _documentDbProvider;
        private readonly IAddressPatchService _addressPatchService;
        private readonly ILogger _logger;

        public PatchAddressHttpTriggerService(IDocumentDBProvider documentDbProvider, IAddressPatchService addressPatchService, ILogger logger)
        {
            _documentDbProvider = documentDbProvider;
            _addressPatchService = addressPatchService;
            _logger = logger;
        }

        public string PatchResource(string addressJson, AddressPatch addressPatch)
        {
            _logger.LogInformation("started patching address");
            if (string.IsNullOrEmpty(addressJson))
            {
                _logger.LogInformation("Can't patch address because input address json is null");
                return null;
            }   

            if (addressPatch == null)
            {
                _logger.LogInformation("Can't patch address because input addressPatch object is null");
                return null;
            }   

            addressPatch.SetDefaultValues();
            var addressObj = _addressPatchService.Patch(addressJson, addressPatch);

            _logger.LogInformation("completed patching address");
            
            return addressObj;
        }

        public async Task<Models.Address> UpdateCosmosAsync(string addressJson, Guid addressId)
        {
            _logger.LogInformation($"started updating address in Cosmos DB for Id [{addressId}]");

            var response = await _documentDbProvider.UpdateAddressAsync(addressJson, addressId);

            var responseStatusCode = response?.StatusCode;

            _logger.LogInformation($"Completed updating address in Cosmos DB. Response Code [{responseStatusCode}]");

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