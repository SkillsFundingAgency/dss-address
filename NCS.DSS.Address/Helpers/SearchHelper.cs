using System;
using Azure;
using Azure.Search.Documents;

namespace NCS.DSS.Address.Helpers
{
    public static class SearchHelper
    {
        private static readonly string SearchServiceName = Environment.GetEnvironmentVariable("SearchServiceName");
        private static readonly string SearchServiceKey = Environment.GetEnvironmentVariable("SearchServiceAdminApiKey");
        private static readonly string SearchServiceIndexName = Environment.GetEnvironmentVariable("CustomerSearchIndexName");

        private static SearchClient _client;

        public static SearchClient GetSearchServiceClient()
        {
            if (_client != null)
                return _client;

            var searchServiceEndpoint = $"https://{SearchServiceName}.search.windows.net";
            _client = new SearchClient(new Uri(searchServiceEndpoint), SearchServiceIndexName, new AzureKeyCredential(SearchServiceKey));

            return _client;
        }
    }
}