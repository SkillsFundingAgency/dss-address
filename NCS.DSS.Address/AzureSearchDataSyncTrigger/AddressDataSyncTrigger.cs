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

            // Add input paramenters to the log message
            var inputMessage =  "Input Paramenters for CosmosDBTrigger" + Environment.NewLine;
            inputMessage += string.Format("ConnectionStringSetting:{0}, LeaseCollectionName: {1}",ConnectionStringSetting,LeaseCollectionName) + Environment.NewLine;
            inputMessage += string.Format("CreateLeaseCollectionIfNotExists:{0}",CreateLeaseCollectionIfNotExists) + Environment.NewLine;
            inputMessage += string.Format("Number of Documents:{0}",documents.Count);
            log.LogInformation(inputMessage);

            SearchHelper.GetSearchServiceClient();

            log.LogInformation("get search service client");
                      
            var client = SearchHelper.GetSearchServiceClient();

            log.LogInformation("get index client");
            
            log.LogInformation("Documents modified " + documents.Count);

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
                    log.LogInformation("attempting to merge docs to azure search");                    

                    var results = await client.IndexDocumentsAsync(batch);

                    var failed = results.Value.Results.Where(r => !r.Succeeded).Select(r => r.Key).ToList();

                    if (failed.Any())
                    {
                        log.LogError(string.Format("Failed to index some of the documents: {0}", string.Join(", ", failed)));
                    }                    

                    log.LogInformation("successfully merged docs to azure search");
                }
                catch (RequestFailedException e)
                {
                    // Added Error Code to the error message
                    log.LogError(string.Format("Failed to index some of the documents. Error Code: {0}",e.ErrorCode));
                    log.LogError(e.ToString());
                }
            }
        }
    }
}