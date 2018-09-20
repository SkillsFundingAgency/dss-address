using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using NCS.DSS.Address.Cosmos.Client;
using NCS.DSS.Address.Cosmos.Helper;

namespace NCS.DSS.Address.Cosmos.Provider
{
    public class DocumentDBProvider : IDocumentDBProvider
    {
        public async Task<bool> DoesCustomerResourceExist(Guid customerId)
        {
            var documentUri = DocumentDBHelper.CreateCustomerDocumentUri(customerId);

            var client = DocumentDBClient.CreateDocumentClient();

            if (client == null)
                return false;
            try
            {
                var response = await client.ReadDocumentAsync(documentUri);
                if (response.Resource != null)
                    return true;
            }
            catch (DocumentClientException)
            {
                return false;
            }

            return false;
        }

        public async Task<bool> DoesCustomerHaveATerminationDate(Guid customerId)
        {
            var documentUri = DocumentDBHelper.CreateCustomerDocumentUri(customerId);

            var client = DocumentDBClient.CreateDocumentClient();

            if (client == null)
                return false;

            try
            {
                var response = await client.ReadDocumentAsync(documentUri);

                var dateOfTermination = response.Resource?.GetPropertyValue<DateTime?>("DateOfTermination");

                return dateOfTermination.HasValue;
            }
            catch (DocumentClientException)
            {
                return false;
            }
        }

        public async Task<ResourceResponse<Document>> GetAddressAsync(Guid addressId)
        {
            var documentUri = DocumentDBHelper.CreateDocumentUri(addressId);

            var client = DocumentDBClient.CreateDocumentClient();

            if (client == null)
                return null;

            var response = await client.ReadDocumentAsync(documentUri);

            return response;
        }

        public async Task<Models.Address> GetAddressForCustomerAsync(Guid customerId, Guid addressId)
        {
            var collectionUri = DocumentDBHelper.CreateDocumentCollectionUri();

            var client = DocumentDBClient.CreateDocumentClient();

            var addressForCustomerQuery = client
                ?.CreateDocumentQuery<Models.Address>(collectionUri, new FeedOptions { MaxItemCount = 1 })
                .Where(x => x.CustomerId == customerId && x.AddressId == addressId)
                .AsDocumentQuery();

            if (addressForCustomerQuery == null)
                return null;

            var addressess = await addressForCustomerQuery.ExecuteNextAsync<Models.Address>();

            return addressess?.FirstOrDefault();
        }


        public async Task<List<Models.Address>> GetAddressesForCustomerAsync(Guid customerId)
        {
            var collectionUri = DocumentDBHelper.CreateDocumentCollectionUri();

            var client = DocumentDBClient.CreateDocumentClient();

            if (client == null)
                return null;

            var queryAddresses = client.CreateDocumentQuery<Models.Address>(collectionUri)
                .Where(so => so.CustomerId == customerId).AsDocumentQuery();

            var addresses = new List<Models.Address>();

            while (queryAddresses.HasMoreResults)
            {
                var response = await queryAddresses.ExecuteNextAsync<Models.Address>();
                addresses.AddRange(response);
            }

            return addresses.Any() ? addresses : null;

        }

        public async Task<ResourceResponse<Document>> CreateAddressAsync(Models.Address address)
        {

            var collectionUri = DocumentDBHelper.CreateDocumentCollectionUri();

            var client = DocumentDBClient.CreateDocumentClient();

            if (client == null)
                return null;

            var response = await client.CreateDocumentAsync(collectionUri, address);

            return response;

        }

        public async Task<ResourceResponse<Document>> UpdateAddressAsync(Models.Address address)
        {
            var documentUri = DocumentDBHelper.CreateDocumentUri(address.AddressId.GetValueOrDefault());

            var client = DocumentDBClient.CreateDocumentClient();

            if (client == null)
                return null;

            var response = await client.ReplaceDocumentAsync(documentUri, address);

            return response;
        }
    }
}