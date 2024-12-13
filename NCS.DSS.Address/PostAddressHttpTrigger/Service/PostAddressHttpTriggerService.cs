using Microsoft.Extensions.Logging;
using NCS.DSS.Address.Cosmos.Provider;
using NCS.DSS.Address.ServiceBus;
using System.Net;

namespace NCS.DSS.Address.PostAddressHttpTrigger.Service
{
    public class PostAddressHttpTriggerService : IPostAddressHttpTriggerService
    {
        private readonly ICosmosDbProvider _cosmosDbProvider;
        private readonly IAddressServiceBusClient _addressServiceBusClient;
        private readonly ILogger<PostAddressHttpTriggerService> _logger;

        public PostAddressHttpTriggerService(ICosmosDbProvider cosmosDbProvider, IAddressServiceBusClient addressServiceBusClient, ILogger<PostAddressHttpTriggerService> logger)
        {
            _cosmosDbProvider = cosmosDbProvider;
            _addressServiceBusClient = addressServiceBusClient;
            _logger = logger;
        }

        public async Task<Models.Address> CreateAsync(Models.Address address, ILogger logger)
        {
            logger.LogInformation("Started creating address with Address POST request");

            if (address == null)
            {
                logger.LogInformation("Address can't be created because input address object is null");
                return null;
            }

            address.SetDefaultValues();

            var response = await _cosmosDbProvider.CreateAddressAsync(address);

            if (response?.StatusCode == HttpStatusCode.Created)
            {
                logger.LogInformation("Completed creating address with Address POST request. Response Code {responseStatusCode}", response.StatusCode);
                return response.Resource;
            }

            _logger.LogError("Failed to create address with ID: {AddressId}.", address.AddressId);
            return null;
        }

        public async Task SendToServiceBusQueueAsync(Models.Address address, string reqUrl)
        {
            try
            {
                _logger.LogInformation("Sending newly created address with ID: {AddressId} to Service Bus for customer ID: {CustomerId}.", address.AddressId, address.CustomerId);

                await _addressServiceBusClient.SendPostMessageAsync(address, reqUrl);

                _logger.LogInformation("Successfully sent address with ID: {AddressId} to Service Bus for customer ID: {CustomerId}.", address.AddressId, address.CustomerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while sending address with ID: {AddressId} to Service Bus for customer ID: {CustomerId}.", address.AddressId, address.CustomerId);
                throw;
            }
        }
    }
}