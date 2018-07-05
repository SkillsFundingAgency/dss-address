using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NCS.DSS.Address.Cosmos.Provider;

namespace NCS.DSS.Address.GetAddressHttpTrigger.Service
{
    public class GetAddressHttpTriggerService : IGetAddressHttpTriggerService
    {
        public async Task<List<Models.Address>> GetAddressesAsync(Guid customerId)
        {
            var documentDbProvider = new DocumentDBProvider();
            var customerAddresses = await documentDbProvider.GetAddressesForCustomerAsync(customerId);

            return customerAddresses.Any() ? customerAddresses : null;
        }
    }
}
