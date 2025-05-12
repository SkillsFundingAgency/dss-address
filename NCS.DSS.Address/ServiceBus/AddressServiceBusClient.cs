using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NCS.DSS.Address.Models;
using Newtonsoft.Json;
using System.Text;

namespace NCS.DSS.Address.ServiceBus
{
    public class AddressServiceBusClient : IAddressServiceBusClient
    {
        private readonly ServiceBusClient _serviceBusClient;
        private readonly ILogger<AddressServiceBusClient> _logger;
        private readonly string _queueName;

        public AddressServiceBusClient(ServiceBusClient serviceBusClient,
            IOptions<AddressConfigurationSettings> configOptions,
            ILogger<AddressServiceBusClient> logger)
        {
            var config = configOptions.Value;
            if (string.IsNullOrEmpty(config.QueueName))
            {
                throw new ArgumentNullException(nameof(config.QueueName), "QueueName cannot be null or empty.");
            }

            _serviceBusClient = serviceBusClient;
            _queueName = config.QueueName;
            _logger = logger;
        }

        public async Task SendPostMessageAsync(Models.Address address, string reqUrl)
        {
            var serviceBusSender = _serviceBusClient.CreateSender(_queueName);

            var messageModel = new MessageModel()
            {
                TitleMessage = "New Address record {" + address.AddressId + "} added for {" + address.CustomerId + "} at " + DateTime.UtcNow,
                CustomerGuid = address.CustomerId,
                LastModifiedDate = address.LastModifiedDate,
                URL = reqUrl + "/" + address.AddressId,
                IsNewCustomer = false,
                TouchpointId = address.LastModifiedTouchpointId
            };

            var msg = new ServiceBusMessage(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(messageModel)))
            {
                ContentType = "application/json",
                MessageId = address.CustomerId + " " + DateTime.UtcNow
            };


            _logger.LogInformation("Attempting to send POST message to service bus. Address ID: {AddressId}", address.AddressId);

            await serviceBusSender.SendMessageAsync(msg);

            _logger.LogInformation("Successfully sent POST message to the service bus. Address ID: {AddressId}", address.AddressId);
        }

        public async Task SendPatchMessageAsync(Models.Address address, Guid customerId, string reqUrl)
        {
            var serviceBusSender = _serviceBusClient.CreateSender(_queueName);

            var messageModel = new MessageModel
            {
                TitleMessage = "Address record modification for {" + customerId + "} at " + DateTime.UtcNow,
                CustomerGuid = customerId,
                LastModifiedDate = address.LastModifiedDate,
                URL = reqUrl,
                IsNewCustomer = false,
                TouchpointId = address.LastModifiedTouchpointId
            };

            var msg = new ServiceBusMessage(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(messageModel)))
            {
                ContentType = "application/json",
                MessageId = customerId + " " + DateTime.UtcNow
            };

            _logger.LogInformation("Attempting to send PATCH message to service bus. Address ID: {AddressId}", address.AddressId);

            await serviceBusSender.SendMessageAsync(msg);

            _logger.LogInformation("Successfully sent PATCH message to the service bus. Address ID: {AddressId}", address.AddressId);
        }
    }
}