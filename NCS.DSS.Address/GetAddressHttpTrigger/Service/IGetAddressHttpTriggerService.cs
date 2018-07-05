using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NCS.DSS.Address.GetAddressHttpTrigger.Service
{
    public interface IGetAddressHttpTriggerService
    {
        Task<List<Models.Address>> GetAddressesAsync(Guid customerId);
        //List<Models.Address> GetAddressesAsync(Guid customerId);
    }
}