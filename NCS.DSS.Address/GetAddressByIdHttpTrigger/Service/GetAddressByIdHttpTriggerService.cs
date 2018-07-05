using System;
using System.Threading.Tasks;
using NCS.DSS.Address.Cosmos.Provider;

namespace NCS.DSS.Address.GetAddressByIdHttpTrigger.Service
{
    public class GetAddressByIdHttpTriggerService : IGetAddressByIdHttpTriggerService
    {
        public async Task<Models.Address> GetAddressForCustomerAsync(Guid customerId, Guid addressId)
        {
            var documentDbProvider = new DocumentDBProvider();
            var address = await documentDbProvider.GetAddressForCustomerAsync(customerId, addressId);

            return address;
        }

    }
}