using Microsoft.Extensions.Logging;
using NCS.DSS.Address.Cosmos.Provider;

namespace NCS.DSS.Address.GetAddressHttpTrigger.Service
{
    public class GetAddressHttpTriggerService : IGetAddressHttpTriggerService
    {

        private readonly ICosmosDbProvider _cosmosDbProvider;
        private readonly ILogger<GetAddressHttpTriggerService> _logger;

        public GetAddressHttpTriggerService(ICosmosDbProvider cosmosDbProvider, ILogger<GetAddressHttpTriggerService> logger)
        {
            _cosmosDbProvider = cosmosDbProvider;
            _logger = logger;
        }

        public async Task<List<Models.Address>> GetAddressesAsync(Guid customerId)
        {
            _logger.LogInformation("Retrieving addresses for customer ID: {CustomerId}.", customerId);

            if (customerId == Guid.Empty)
            {
                _logger.LogWarning("Invalid customer ID provided: {CustomerId}.", customerId);
                return null;
            }

            var customerAddresses = await _cosmosDbProvider.GetAddressesForCustomerAsync(customerId);

            if (customerAddresses == null || customerAddresses.Count == 0)
            {
                _logger.LogInformation("No addresses found for customer ID: {CustomerId}.", customerId);
            }
            else
            {
                _logger.LogInformation("Successfully retrieved address(es) for customer ID: {CustomerId}.", customerId);
            }

            return customerAddresses;
        }
    }
}