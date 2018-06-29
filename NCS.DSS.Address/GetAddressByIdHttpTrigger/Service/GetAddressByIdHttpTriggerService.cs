using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Client;

namespace NCS.DSS.Address.GetAddressByIdHttpTrigger.Service
{
    public class GetAddressByIdHttpTriggerService : IGetAddressByIdHttpTriggerService
    {

        private readonly DocumentClient _client = new DocumentClient(
            new Uri(ConfigurationManager.AppSettings["DBEndpoint"]),
            ConfigurationManager.AppSettings["DBKey"]);

        private readonly string databaseId = ConfigurationManager.AppSettings["DatabaseId"];
        private readonly string collectionId = ConfigurationManager.AppSettings["CollectionId"];


        public async Task<Models.Address> GetAddressAsync(Guid addressId)
        {
            var response = await _client.ReadDocumentAsync(UriFactory.CreateDocumentUri(databaseId, collectionId, addressId.ToString()));

            await Task.FromResult(response);

            if (response.Resource == null)
                return null;

            var address = (Models.Address)(dynamic)response.Resource;

            return address;

        }

    }
}