using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using NCS.DSS.Address.Cosmos.Client;
using NCS.DSS.Address.Cosmos.Helper;

namespace NCS.DSS.Address.Cosmos.Provider
{
    public class DocumentDBProvider : IDocumentDBProvider
    {
        private readonly DocumentDBHelper _documentDbHelper;
        private readonly DocumentDBClient _databaseClient;

        public DocumentDBProvider()
        {
            _documentDbHelper = new DocumentDBHelper();
            _databaseClient = new DocumentDBClient();
        }

        public bool DoesCustomerResourceExist(Guid customerId)
        {
            var collectionUri = _documentDbHelper.CreateDocumentCollectionUri();

            var client = _databaseClient.CreateDocumentClient();

            if (client == null)
                return false;

            var query = client.CreateDocumentQuery<Models.Address>(collectionUri, new FeedOptions {MaxItemCount = 1});
            var customerExists = query.Where(x => x.CustomerId == customerId).AsEnumerable().Any();

            return customerExists;
        }

        public async Task<ResourceResponse<Document>> GetAddressAsync(Guid addressId)
        {
            var documentUri = _documentDbHelper.CreateDocumentUri(addressId);

            var client = _databaseClient.CreateDocumentClient();

            if (client == null)
                return null;

            var response = await client.ReadDocumentAsync(documentUri);

            return response;
        }
        
        public List<Models.Address> GetAddressesForCustomer(Guid customerId)
        {
            var collectionUri = _documentDbHelper.CreateDocumentCollectionUri();

            var client = _databaseClient.CreateDocumentClient();

            if (client == null)
                return null;

            var queryAddresses = client.CreateDocumentQuery<Models.Address>(collectionUri)
                .Where(so => so.CustomerId == customerId).ToList();

            return queryAddresses.Any() ? queryAddresses : null;

        }

        public async Task<ResourceResponse<Document>> CreateAddressAsync(Models.Address address)
        {

            var collectionUri = _documentDbHelper.CreateDocumentCollectionUri();

            var client = _databaseClient.CreateDocumentClient();

            if (client == null)
                return null;

            var response = await client.CreateDocumentAsync(collectionUri, address);

            return response;

        }

        public async Task<ResourceResponse<Document>> UpdateAddressAsync(Models.Address address)
        {
            var documentUri = _documentDbHelper.CreateDocumentUri(address.AddressId.GetValueOrDefault());

            var client = _databaseClient.CreateDocumentClient();

            if (client == null)
                return null;

            var response = await client.ReplaceDocumentAsync(documentUri, address);

            return response;
        }
    }
}