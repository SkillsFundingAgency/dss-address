using Microsoft.Extensions.Logging;
using NCS.DSS.Address.Cosmos.Provider;

namespace NCS.DSS.Address.GetAddressByIdHttpTrigger.Service
{
    public class GetAddressByIdHttpTriggerService : IGetAddressByIdHttpTriggerService
    {
        private readonly ICosmosDbProvider _cosmosDbProvider;
        private readonly ILogger<GetAddressByIdHttpTriggerService> _logger;

        public GetAddressByIdHttpTriggerService(ICosmosDbProvider cosmosDbProvider, ILogger<GetAddressByIdHttpTriggerService> logger)
        {
            _cosmosDbProvider = cosmosDbProvider;
            _logger = logger;
        }

        public async Task<Models.Address> GetAddressForCustomerAsync(Guid customerId, Guid addressId)
        {
            _logger.LogInformation("Retrieving address with ID: {AddressId} for customer ID: {CustomerId}.", addressId, customerId);

            if (customerId == Guid.Empty)
            {
                _logger.LogWarning("Invalid customer ID provided: {CustomerId}.", customerId);
                return null;
            }

            if (addressId == Guid.Empty)
            {
                _logger.LogWarning("Invalid address ID provided: {AddressId}.", addressId);
                return null;
            }

            var address = await _cosmosDbProvider.GetAddressForCustomerAsync(customerId, addressId);

            if (address == null)
            {
                _logger.LogInformation("No address found with ID: {AddressId} for customer ID: {CustomerId}", addressId, customerId);
            }
            else
            {
                _logger.LogInformation("Successfully retrieved address with ID: {AddressId} for customer ID: {CustomerId}", addressId, customerId);
            }

            return address;
        }
    }
}