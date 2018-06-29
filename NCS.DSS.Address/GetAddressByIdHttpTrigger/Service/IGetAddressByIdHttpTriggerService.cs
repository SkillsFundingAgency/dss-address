using System;
using System.Threading.Tasks;

namespace NCS.DSS.Address.GetAddressByIdHttpTrigger.Service
{
    public interface IGetAddressByIdHttpTriggerService
    {
        Task<Models.Address> GetAddressAsync(Guid addressId);
    }
}