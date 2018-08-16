using System;
using System.Net;
using System.Threading.Tasks;
using NCS.DSS.Address.Cosmos.Provider;
using NCS.DSS.Address.ServiceBus;

namespace NCS.DSS.Address.PostAddressHttpTrigger.Service
{
    public class PostAddressHttpTriggerService : IPostAddressHttpTriggerService
    {
        public async Task<Models.Address> CreateAsync(Models.Address address)
        {
            if (address == null)
                return null;

            address.SetDefaultValues();

            var documentDbProvider = new DocumentDBProvider();

            var response = await documentDbProvider.CreateAddressAsync(address);

            return response.StatusCode == HttpStatusCode.Created ? (dynamic) response.Resource : (Guid?) null;
        }

        public async Task SendToServiceBusQueueAsync(Models.Address address, string reqUrl)
        {
            await ServiceBusClient.SendPostMessageAsync(address, reqUrl);
        }
    }
}