using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace NCS.DSS.Address.Cosmos.Helper
{
    public interface IResourceHelper
    {
        Task<bool> DoesCustomerExist(Guid customerId);
        Task<bool> DoesCustomerExist(Guid customerId, ILogger logger);
        Task<bool> IsCustomerReadOnly(Guid customerId);
    }
}