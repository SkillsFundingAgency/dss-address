using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure;
using Azure.Search.Documents.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using NCS.DSS.Address.Helpers;
using Document = Microsoft.Azure.Documents.Document;

namespace NCS.DSS.Address.AzureSearchDataSyncTrigger
{
    public static class AddressDataSyncTrigger
    {
        [FunctionName("SyncAddressDataSyncTrigger")]
        public static async Task Run([CosmosDBTrigger("addresses", "addresses", ConnectionStringSetting = "AddressConnectionString",
                LeaseCollectionName = "addresses-leases", CreateLeaseCollectionIfNotExists = true)]IReadOnlyList<Document> documents,
            ILogger log)
        {
            log.LogInformation("Entered SyncDataForCustomerSearchTrigger");

            SearchHelper.GetSearchServiceClient();

            log.LogInformation("get search service client");

            var indexClient = SearchHelper.GetIndexClient();

            log.LogInformation("get index client");
            
            log.LogInformation("Documents modified " + documents.Count);

            if (documents.Count > 0)
            {
                var address = documents.Select(doc => new Models.Address()
                    {
                        CustomerId = doc.GetPropertyValue<Guid>("CustomerId"),
                        Address1 = doc.GetPropertyValue<string>("Address1"),
                        PostCode = doc.GetPropertyValue<string>("PostCode")
                    })
                    .ToList();

                var batch = IndexDocumentsBatch.MergeOrUpload(address);

                try
                {
                    log.LogInformation("attempting to merge docs to azure search");

                    await indexClient.IndexDocumentsAsync(batch);

                    log.LogInformation("successfully merged docs to azure search");

                }
                catch (RequestFailedException e)
                {
                    log.LogError(string.Format("Failed to index some of the documents."));

                    log.LogError(e.ToString());
                }
            }
        }
    }
}