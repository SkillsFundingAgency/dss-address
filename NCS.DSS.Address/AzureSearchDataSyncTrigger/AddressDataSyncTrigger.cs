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
            _logger.LogInformation("Function {FunctionName} has been invoked", nameof(AddressDataSyncTrigger));

            var inputMessage = "Input Paramenters " + Environment.NewLine;
            inputMessage += string.Format("Number of Documents:{0}", documents.Count);

            _logger.LogInformation(inputMessage);

            _logger.LogInformation("Get search service client");
            var client = SearchHelper.GetSearchServiceClient(_logger);

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
                    _logger.LogInformation("Attempting to merge documents to azure search");

                    var results = await client.IndexDocumentsAsync(batch);

                    var failed = results.Value.Results.Where(r => !r.Succeeded).Select(r => r.Key).ToList();

                    if (failed.Any())
                    {
                        _logger.LogError("Failed to index some of the documents: {0}", string.Join(", ", failed));
                    }

                    _logger.LogInformation("Successfully merged documents to azure search");
                }
                catch (RequestFailedException ex)
                {
                    _logger.LogError(ex, "Failed to index some of the documents. Error Code: {0}. Error Message: {1}", ex.ErrorCode, ex.Message);
                }
            }
        }
    }
}