using Microsoft.Extensions.Logging;
using NCS.DSS.Address.Cosmos.Provider;
using NCS.DSS.Address.ServiceBus;
using System.Net;
using System.Threading.Tasks;

namespace NCS.DSS.Address.PostAddressHttpTrigger.Service
{
    public class PostAddressHttpTriggerService : IPostAddressHttpTriggerService
    {

        private readonly IDocumentDBProvider _documentDbProvider;
        public PostAddressHttpTriggerService(IDocumentDBProvider documentDbProvider)
        {
            _documentDbProvider = documentDbProvider;
        }

        public async Task<Models.Address> CreateAsync(Models.Address address, ILogger logger)
        {
            logger.LogInformation("Started creating address with Address POST request");

            if (address == null)
            {
                logger.LogInformation("Address Can't be created because input address object is null");
                return null;
            }

            address.SetDefaultValues();

            var response = await _documentDbProvider.CreateAddressAsync(address);

            logger.LogInformation($"Completed creating address with Address POST request. Response Code [{response.StatusCode}]");

            return response.StatusCode == HttpStatusCode.Created ? (dynamic)response.Resource : null;
        }

        public async Task SendToServiceBusQueueAsync(Models.Address address, string reqUrl)
        {
            await ServiceBusClient.SendPostMessageAsync(address, reqUrl);
        }
    }
}