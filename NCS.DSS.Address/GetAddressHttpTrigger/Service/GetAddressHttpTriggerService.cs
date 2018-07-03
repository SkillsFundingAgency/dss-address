using System;
using System.Collections.Generic;
using System.Linq;
using NCS.DSS.Address.Cosmos.Provider;

namespace NCS.DSS.Address.GetAddressHttpTrigger.Service
{
    public class GetAddressHttpTriggerService : IGetAddressHttpTriggerService
    {
        public List<Models.Address> GetAddressesAsync(Guid customerId)
        {
            var documentDbProvider = new DocumentDBProvider();
            var customerAddresses = documentDbProvider.GetAddressesForCustomer(customerId);

            return customerAddresses.Any() ? customerAddresses : null;
        }
    }
}
