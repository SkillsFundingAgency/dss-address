using System;
using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;

namespace NCS.DSS.Address.Helpers
{
    public static class SearchHelper
    {
        private static readonly string SearchServiceName = Environment.GetEnvironmentVariable("SearchServiceName");
        private static readonly string SearchServiceKey = Environment.GetEnvironmentVariable("SearchServiceAdminApiKey");
        private static readonly string SearchServiceIndexName = Environment.GetEnvironmentVariable("CustomerSearchIndexName");

        private static SearchIndexClient _serviceClient;
        private static SearchClient _indexClient;

        public static SearchIndexClient GetSearchServiceClient()
        {
            if (_serviceClient != null)
                return _serviceClient;            

            Uri endpoint = new Uri($"https://{SearchServiceName}.search.windows.net");
            AzureKeyCredential credential = new AzureKeyCredential(SearchServiceKey);
            _serviceClient = new SearchIndexClient(endpoint, credential);            

            return _serviceClient;
        }
        
        public static SearchClient GetIndexClient()
        {
            if (_indexClient != null)
                return _indexClient;

            //_indexClient = _serviceClient?.Indexes?.GetClient(SearchServiceIndexName);
            _indexClient = new SearchClient(new Uri($"https://{SearchServiceName}.search.windows.net"), SearchServiceIndexName, new AzureKeyCredential(SearchServiceKey));



            return _indexClient;
        }

    }
}