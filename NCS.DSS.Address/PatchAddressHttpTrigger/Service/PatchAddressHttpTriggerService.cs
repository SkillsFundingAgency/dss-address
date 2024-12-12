using Microsoft.Extensions.Logging;
using NCS.DSS.Address.Cosmos.Provider;
using NCS.DSS.Address.Models;
using NCS.DSS.Address.ServiceBus;
using System.Net;

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

        public string PatchResource(string addressJson, AddressPatch addressPatch, ILogger logger)
        {
            logger.LogInformation("started patching address");
            if (string.IsNullOrEmpty(addressJson))
            {
                logger.LogInformation("Can't patch address because input address json is null");
                return null;
            }

            if (addressPatch == null)
            {
                logger.LogInformation("Can't patch address because input addressPatch object is null");
                return null;
            }

            addressPatch.SetDefaultValues();
            var addressObj = _addressPatchService.Patch(addressJson, addressPatch, logger);

            logger.LogInformation("completed patching address");

            return addressObj;
        }

        public async Task<Models.Address> UpdateCosmosAsync(string addressJson, Guid addressId, ILogger logger)
        {
            logger.LogInformation($"started updating address in Cosmos DB for Id [{addressId}]");

            var response = await _documentDbProvider.UpdateAddressAsync(addressJson, addressId);

            var responseStatusCode = response?.StatusCode;

            logger.LogInformation($"Completed updating address in Cosmos DB. Response Code [{responseStatusCode}]");

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