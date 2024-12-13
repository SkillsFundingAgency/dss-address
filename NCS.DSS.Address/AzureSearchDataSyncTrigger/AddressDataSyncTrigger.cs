using Azure;
using Azure.Search.Documents.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.Address.Helpers;
using NCS.DSS.Address.Models;

namespace NCS.DSS.Address.AzureSearchDataSyncTrigger
{
    public class AddressDataSyncTrigger
    {
        private readonly ILogger<AddressDataSyncTrigger> _logger;

        public AddressDataSyncTrigger(ILogger<AddressDataSyncTrigger> log)
        {
            _logger = log;
        }
        [Function("SyncAddressDataSyncTrigger")]
        public async Task Run([CosmosDBTrigger("addresses", "addresses", Connection = "AddressConnectionString",
                LeaseContainerName = "addresses-leases", CreateLeaseContainerIfNotExists = true)]IReadOnlyList<AddressDocument> documents)
        {
            _logger.LogInformation("Entered SyncDataForCustomerSearchTrigger");

            var inputMessage = "Input Paramenters " + Environment.NewLine;
            inputMessage += string.Format("Number of Documents:{0}", documents.Count);

            _logger.LogInformation(inputMessage);

            SearchHelper.GetSearchServiceClient(_logger);

            _logger.LogInformation("get search service client");

            var client = SearchHelper.GetSearchServiceClient(_logger);

            _logger.LogInformation("get index client");

            _logger.LogInformation("Documents modified " + documents.Count);

            if (documents.Count > 0)
            {
                var address = documents.Select(doc => new
                {
                    doc.CustomerId,
                    doc.Address1,
                    doc.PostCode
                })
                    .ToList();

                var batch = IndexDocumentsBatch.MergeOrUpload(address);

                try
                {
                    _logger.LogInformation("attempting to merge docs to azure search");

                    var results = await client.IndexDocumentsAsync(batch);

                    var failed = results.Value.Results.Where(r => !r.Succeeded).Select(r => r.Key).ToList();

                    if (failed.Any())
                    {
                        _logger.LogError(string.Format("Failed to index some of the documents: {0}", string.Join(", ", failed)));
                    }

                    _logger.LogInformation("successfully merged docs to azure search");
                }
                catch (RequestFailedException e)
                {
                    _logger.LogError(string.Format("Failed to index some of the documents. Error Code: {0}", e.ErrorCode));
                    _logger.LogError(e.ToString());
                }
            }
        }
    }
}