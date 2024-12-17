using Microsoft.Extensions.Logging;
using NCS.DSS.Address.Cosmos.Provider;
using NCS.DSS.Address.Models;
using NCS.DSS.Address.ServiceBus;
using System.Net;

namespace NCS.DSS.Address.PatchAddressHttpTrigger.Service
{
    public class PatchAddressHttpTriggerService : IPatchAddressHttpTriggerService
    {
        private readonly ICosmosDbProvider _cosmosDbProvider;
        private readonly IAddressPatchService _addressPatchService;
        private readonly IAddressServiceBusClient _addressServiceBusClient;
        private readonly ILogger<PatchAddressHttpTriggerService> _logger;

        public PatchAddressHttpTriggerService(ICosmosDbProvider cosmosDbProvider, IAddressPatchService addressPatchService, IAddressServiceBusClient addressServiceBusClient, ILogger<PatchAddressHttpTriggerService> logger)
        {
            _cosmosDbProvider = cosmosDbProvider;
            _addressPatchService = addressPatchService;
            _addressServiceBusClient = addressServiceBusClient;
            _logger = logger;
        }

        public string PatchResource(string addressJson, AddressPatch addressPatch)
        {
            _logger.LogInformation("Started patching address");
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

            _logger.LogInformation("Setting default values for address PATCH object.");
            addressPatch.SetDefaultValues();
            _logger.LogInformation("Default values for address PATCH object are successfully set.");

            var addressObj = _addressPatchService.Patch(addressJson, addressPatch);

            _logger.LogInformation("Completed patching address");

            return addressObj;
        }

        public async Task<Models.Address> UpdateCosmosAsync(string addressJson, Guid addressId)
        {
            if (string.IsNullOrEmpty(addressJson))
            {
                _logger.LogInformation("The address object provided is either null or empty.");
                return null;
            }

            _logger.LogInformation("Started updating address in Cosmos DB with ID: {addressId}", addressId);

            var response = await _cosmosDbProvider.UpdateAddressAsync(addressJson, addressId);

            if (response?.StatusCode == HttpStatusCode.OK)
            {
                _logger.LogInformation("Completed updating address in Cosmos DB with ID: {addressId}", addressId);
                return response.Resource;
            }

            _logger.LogError("Failed to update address in Cosmos DB with ID: {addressId}.", addressId);
            return null;
        }

        public async Task<string> GetAddressForCustomerAsync(Guid customerId, Guid addressId)
        {
            return await _cosmosDbProvider.GetAddressByIdForUpdateAsync(customerId, addressId);
        }

        public async Task SendToServiceBusQueueAsync(Models.Address address, Guid customerId, string reqUrl)
        {
            try
            {
                _logger.LogInformation("Sending address with ID: {AddressId} to Service Bus for customer ID: {CustomerId}.", address.AddressId, customerId);

                await _addressServiceBusClient.SendPatchMessageAsync(address, customerId, reqUrl);

                _logger.LogInformation("Successfully sent address with ID: {AddressId} to Service Bus for customer ID: {CustomerId}.", address.AddressId, customerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while sending address with ID: {AddressId} to Service Bus for customer ID: {CustomerId}.", address.AddressId, customerId);
                throw;
            }
        }
    }
}