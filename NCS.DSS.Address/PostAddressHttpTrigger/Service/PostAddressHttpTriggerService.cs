using System;
using System.Net;
using System.Threading.Tasks;
using NCS.DSS.Address.Cosmos.Provider;
using NCS.DSS.Address.ServiceBus;
using Microsoft.Extensions.Logging;

namespace NCS.DSS.Address.PostAddressHttpTrigger.Service
{
    public class PostAddressHttpTriggerService : IPostAddressHttpTriggerService
    {

        private readonly IDocumentDBProvider _documentDbProvider;
        private readonly ILogger _logger;
        public PostAddressHttpTriggerService(IDocumentDBProvider documentDbProvider, ILogger logger)
        {
            _documentDbProvider = documentDbProvider;
            _logger = logger;
        }

        public async Task<Models.Address> CreateAsync(Models.Address address)
        {
            _logger.LogInformation("Started creating address with Address POST request");

            if (address == null)
            {
                _logger.LogInformation("Address Can't be created because input address object is null");
                return null;
            }    

            address.SetDefaultValues();

            var response = await _documentDbProvider.CreateAddressAsync(address);

            _logger.LogInformation($"Completed creating address with Address POST request. Response Code [{response.StatusCode}]");

            return response.StatusCode == HttpStatusCode.Created ? (dynamic) response.Resource : null;
        }

        public async Task SendToServiceBusQueueAsync(Models.Address address, string reqUrl)
        {
            await ServiceBusClient.SendPostMessageAsync(address, reqUrl);
        }
    }
}