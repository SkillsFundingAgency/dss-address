using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Client;

namespace NCS.DSS.Address.GetAddressHttpTrigger.Service
{
    public class GetAddressHttpTriggerService : IGetAddressHttpTriggerService
    {

        private readonly DocumentClient _client = new DocumentClient(
            new Uri(ConfigurationManager.AppSettings["DBEndpoint"]),
            ConfigurationManager.AppSettings["DBKey"]);

        private readonly string _databaseId = ConfigurationManager.AppSettings["DatabaseId"];
        private readonly string _collectionId = ConfigurationManager.AppSettings["CollectionId"];

        public async Task<List<Models.Address>> GetAddressesAsync(Guid customerId)
        {
            var collectionLink = UriFactory.CreateDocumentCollectionUri(_databaseId, _collectionId);

            var queryAddress = _client.CreateDocumentQuery<Models.Address>(collectionLink)
                .Where(so => so.CustomerId == customerId).ToList();

            await Task.FromResult(queryAddress);

            return queryAddress.Any() ? queryAddress : null;

        }

    }
}
