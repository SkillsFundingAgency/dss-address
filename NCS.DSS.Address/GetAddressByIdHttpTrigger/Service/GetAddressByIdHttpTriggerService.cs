using NCS.DSS.Address.Cosmos.Provider;

namespace NCS.DSS.Address.GetAddressByIdHttpTrigger.Service
{
    public class GetAddressByIdHttpTriggerService : IGetAddressByIdHttpTriggerService
    {

        private readonly IDocumentDBProvider _documentDbProvider;

        public GetAddressByIdHttpTriggerService(IDocumentDBProvider documentDbProvider)
        {
            _documentDbProvider = documentDbProvider;
        }

        public async Task<Models.Address> GetAddressForCustomerAsync(Guid customerId, Guid addressId)
        {
            var address = await _documentDbProvider.GetAddressForCustomerAsync(customerId, addressId);

            return address;
        }
    }
}