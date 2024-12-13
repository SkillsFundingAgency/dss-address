using NCS.DSS.Address.Cosmos.Provider;

namespace NCS.DSS.Address.GetAddressByIdHttpTrigger.Service
{
    public class GetAddressByIdHttpTriggerService : IGetAddressByIdHttpTriggerService
    {

        private readonly ICosmosDbProvider _cosmosDbProvider;

        public GetAddressByIdHttpTriggerService(ICosmosDbProvider cosmosDbProvider)
        {
            _cosmosDbProvider = cosmosDbProvider;
        }

        public async Task<Models.Address> GetAddressForCustomerAsync(Guid customerId, Guid addressId)
        {
            var address = await _cosmosDbProvider.GetAddressForCustomerAsync(customerId, addressId);

            return address;
        }
    }
}