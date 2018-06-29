using System;
using System.Configuration;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Client;

namespace NCS.DSS.Address.PostAddressHttpTrigger.Service
{
    public class PostAddressHttpTriggerService : IPostAddressHttpTriggerService
    {
        private readonly DocumentClient _client = new DocumentClient(
            new Uri(ConfigurationManager.AppSettings["DBEndpoint"]), 
            ConfigurationManager.AppSettings["DBKey"]);

        private readonly string databaseId = ConfigurationManager.AppSettings["DatabaseId"];
        private readonly string collectionId = ConfigurationManager.AppSettings["CollectionId"];


        public async Task<Guid?> CreateAsync(Models.Address address)
        {
            if (address == null)
                return null;

            var collectionLink = UriFactory.CreateDocumentCollectionUri(databaseId, collectionId);

            var addressId = Guid.NewGuid();
            address.AddressId = addressId;

            var created = await _client.CreateDocumentAsync(collectionLink, address);

            return created.StatusCode == HttpStatusCode.Created ? addressId : (Guid?) null;
        }
    }
}