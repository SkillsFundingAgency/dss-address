using System;
using System.Net;
using System.Threading.Tasks;
using NCS.DSS.Address.Cosmos.Provider;
using NCS.DSS.Address.ServiceBus;

namespace NCS.DSS.Address.PostAddressHttpTrigger.Service
{
    public class PostAddressHttpTriggerService : IPostAddressHttpTriggerService
    {

        private readonly IDocumentDBProvider _documentDbProvider;

        public PostAddressHttpTriggerService(IDocumentDBProvider documentDbProvider)
        {
            _documentDbProvider = documentDbProvider;
        }

        public async Task<Models.Address> CreateAsync(Models.Address address)
        {
            if (address == null)
                return null;

            address.SetDefaultValues();

            var response = await _documentDbProvider.CreateAddressAsync(address);

            return response.StatusCode == HttpStatusCode.Created ? (dynamic) response.Resource : null;
        }

        public async Task SendToServiceBusQueueAsync(Models.Address address, string reqUrl)
        {
            await ServiceBusClient.SendPostMessageAsync(address, reqUrl);
        }
    }
}