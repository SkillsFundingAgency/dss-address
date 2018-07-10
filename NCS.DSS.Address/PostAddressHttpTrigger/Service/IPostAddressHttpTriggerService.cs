using System;
using System.Threading.Tasks;

namespace NCS.DSS.Address.PostAddressHttpTrigger.Service
{
    public interface IPostAddressHttpTriggerService
    {
        Task<Models.Address> CreateAsync(Models.Address address);
    }
}