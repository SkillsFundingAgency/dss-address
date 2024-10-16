using Azure;
using Azure.Search.Documents.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.Address.Helpers;
using Document = Microsoft.Azure.Documents.Document;

namespace NCS.DSS.Address.AzureSearchDataSyncTrigger
{
    public class AddressDataSyncTrigger
    {
        private readonly ILogger<AddressDataSyncTrigger> _log;

        public AddressDataSyncTrigger(ILogger<AddressDataSyncTrigger> log)
        {
            _log = log;
        }
        [Function("SyncAddressDataSyncTrigger")]
        public async Task Run([CosmosDBTrigger("addresses", "addresses", Connection = "AddressConnectionString",
                LeaseContainerName = "addresses-leases", CreateLeaseContainerIfNotExists = true)]IReadOnlyList<Document> documents)
        {
            _log.LogInformation("Entered SyncDataForCustomerSearchTrigger");

            var inputMessage = "Input Paramenters " + Environment.NewLine;
            inputMessage += string.Format("Number of Documents:{0}", documents.Count);

            _log.LogInformation(inputMessage);

            SearchHelper.GetSearchServiceClient(_log);

            _log.LogInformation("get search service client");

            var client = SearchHelper.GetSearchServiceClient(_log);

            _log.LogInformation("get index client");

            _log.LogInformation("Documents modified " + documents.Count);

            if (documents.Count > 0)
            {
                var address = documents.Select(doc => new
                {
                    CustomerId = doc.GetPropertyValue<Guid>("CustomerId"),
                    Address1 = doc.GetPropertyValue<string>("Address1"),
                    PostCode = doc.GetPropertyValue<string>("PostCode")
                })
                    .ToList();

                var batch = IndexDocumentsBatch.MergeOrUpload(address);

                try
                {
                    _log.LogInformation("attempting to merge docs to azure search");

                    var results = await client.IndexDocumentsAsync(batch);

                    var failed = results.Value.Results.Where(r => !r.Succeeded).Select(r => r.Key).ToList();

                    if (failed.Any())
                    {
                        _log.LogError(string.Format("Failed to index some of the documents: {0}", string.Join(", ", failed)));
                    }

                    _log.LogInformation("successfully merged docs to azure search");
                }
                catch (RequestFailedException e)
                {
                    _log.LogError(string.Format("Failed to index some of the documents. Error Code: {0}", e.ErrorCode));
                    _log.LogError(e.ToString());
                }
            }
        }
    }
}