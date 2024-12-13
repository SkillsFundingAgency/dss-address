using NCS.DSS.Address.Cosmos.Provider;

namespace NCS.DSS.Address.GetAddressHttpTrigger.Service
{
    public class GetAddressHttpTriggerService : IGetAddressHttpTriggerService
    {

        private readonly ICosmosDbProvider _cosmosDbProvider;

        public GetAddressHttpTriggerService(ICosmosDbProvider cosmosDbProvider)
        {
            _cosmosDbProvider = cosmosDbProvider;
        }

        public async Task<List<Models.Address>> GetAddressesAsync(Guid customerId)
        {
            var customerAddresses = await _cosmosDbProvider.GetAddressesForCustomerAsync(customerId);

            return customerAddresses;
        }
    }
}