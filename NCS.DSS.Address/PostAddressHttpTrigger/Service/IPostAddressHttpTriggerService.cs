using System;
using System.Threading.Tasks;

namespace NCS.DSS.Address.PostAddressHttpTrigger.Service
{
    public interface IPostAddressHttpTriggerService
    {
        Task<Guid?> CreateAsync(Models.Address address);
    }
}