using Azure;
using Azure.Search.Documents;
using Microsoft.Extensions.Logging;
namespace NCS.DSS.Address.Helpers
{
    public static class SearchHelper
    {
        private static readonly string SearchServiceName = Environment.GetEnvironmentVariable("SearchServiceName");
        private static readonly string SearchServiceKey = Environment.GetEnvironmentVariable("SearchServiceAdminApiKey");
        private static readonly string SearchServiceIndexName = Environment.GetEnvironmentVariable("CustomerSearchIndexName");

        private static SearchClient _client;

        public static SearchClient GetSearchServiceClient(ILogger logger)
        {
            logger.LogInformation($"Retrieving Search Service Client with name: {SearchServiceName}");
            if (_client != null)
            {
                logger.LogInformation("Not required. Search Service Client retrieved already");
                return _client;
            }

            var searchServiceEndpoint = $"https://{SearchServiceName}.search.windows.net";
            _client = new SearchClient(new Uri(searchServiceEndpoint), SearchServiceIndexName, new AzureKeyCredential(SearchServiceKey));

            logger.LogInformation($"Successfully retrieved Search Service Client with endpoint: {searchServiceEndpoint}");
            return _client;
        }
    }
}