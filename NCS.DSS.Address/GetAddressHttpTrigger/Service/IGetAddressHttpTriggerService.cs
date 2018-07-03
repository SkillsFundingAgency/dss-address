using System;
using System.Collections.Generic;

namespace NCS.DSS.Address.GetAddressHttpTrigger.Service
{
    public interface IGetAddressHttpTriggerService
    {
        List<Models.Address> GetAddressesAsync(Guid customerId);
    }
}