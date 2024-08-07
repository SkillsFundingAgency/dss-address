using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NCS.DSS.Address.Cosmos.Provider;

namespace NCS.DSS.Address.Cosmos.Helper
{
    public class ResourceHelper : IResourceHelper
    {

        private readonly IDocumentDBProvider _documentDbProvider;

        public ResourceHelper(IDocumentDBProvider documentDbProvider)
        {
            _documentDbProvider = documentDbProvider;
        }

        public async Task<bool> DoesCustomerExist(Guid customerId)
        {
            var doesCustomerExist = await _documentDbProvider.DoesCustomerResourceExist(customerId);

            return doesCustomerExist;
        }

        public async Task<bool> DoesCustomerExist(Guid customerId, ILogger logger)
        {
            var doesCustomerExist = await _documentDbProvider.DoesCustomerResourceExist(customerId, logger);

            return doesCustomerExist;
        }

        public async Task<bool> IsCustomerReadOnly(Guid customerId)
        {
            var isCustomerReadOnly = await _documentDbProvider.DoesCustomerHaveATerminationDate(customerId);

            return isCustomerReadOnly;
        }
    }
}
