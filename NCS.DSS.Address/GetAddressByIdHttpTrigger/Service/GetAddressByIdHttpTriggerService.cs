using System;
using System.Threading.Tasks;
using NCS.DSS.Address.Cosmos.Provider;

namespace NCS.DSS.Address.GetAddressByIdHttpTrigger.Service
{
    public class GetAddressByIdHttpTriggerService : IGetAddressByIdHttpTriggerService
    {
        public async Task<Models.Address> GetAddressAsync(Guid addressId)
        {
            var documentDbProvider = new DocumentDBProvider();
            var response = await documentDbProvider.GetAddressAsync(addressId);

            if (response.Resource == null)
                return null;

            var address = (Models.Address)(dynamic)response.Resource;

            return address;
        }

    }
}