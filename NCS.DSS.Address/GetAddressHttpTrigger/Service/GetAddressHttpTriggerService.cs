using NCS.DSS.Address.Cosmos.Provider;

namespace NCS.DSS.Address.GetAddressHttpTrigger.Service
{
    public class GetAddressHttpTriggerService : IGetAddressHttpTriggerService
    {

        private readonly IDocumentDBProvider _documentDbProvider;

        public GetAddressHttpTriggerService(IDocumentDBProvider documentDbProvider)
        {
            _documentDbProvider = documentDbProvider;
        }

        public async Task<List<Models.Address>> GetAddressesAsync(Guid customerId)
        {
            var customerAddresses = await _documentDbProvider.GetAddressesForCustomerAsync(customerId);

            return customerAddresses;
        }
    }
}