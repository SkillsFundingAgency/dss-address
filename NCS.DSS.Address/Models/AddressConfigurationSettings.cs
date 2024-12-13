namespace NCS.DSS.Address.Models
{
    public class AddressConfigurationSettings
    {
        public required string CollectionId { get; set; }
        public required string CustomerCollectionId { get; set; }
        public required string CustomerDatabaseId { get; set; }
        public required string DatabaseId { get; set; }
        public required string LeaseCollectionName { get; set; }
        public required string LeaseCollectionPrefix { get; set; }
        public required string QueueName { get; set; }
        public required string ServiceBusConnectionString { get; set; }
        public required string ChangeFeedQueueName { get; set; }
        public required string AddressConnectionString { get; set; }
        public required string SearchServiceName { get; set; }
        public required string SearchServiceAdminApiKey { get; set; }
        public required string CustomerSearchIndexName { get; set; }
        public required string AzureMapURL { get; set; }
        public required string AzureMapApiVersion { get; set; }
        public required string AzureMapSubscriptionKey { get; set; }
        public required string AzureCountrySet { get; set; }

    }
}
