using System;
using System.Configuration;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Client;


namespace NCS.DSS.Address.PatchAddressHttpTrigger.Service
{
    public class PatchAddressHttpTriggerService : IPatchAddressHttpTriggerService
    {

        private readonly DocumentClient _client = new DocumentClient(
            new Uri(ConfigurationManager.AppSettings["DBEndpoint"]),
            ConfigurationManager.AppSettings["DBKey"]);

        private readonly string _databaseId = ConfigurationManager.AppSettings["DatabaseId"];
        private readonly string _collectionId = ConfigurationManager.AppSettings["CollectionId"];
        
        public async Task<Models.Address> UpdateAsync(Models.Address address, Models.AddressPatch addressPatch)
        {
            if (address == null)
                return null;

            address.Patch(addressPatch);
                        
            var response = await _client.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(_databaseId, _collectionId, address.AddressId.ToString()), address);

            var responseStatusCode = response.StatusCode;

            return responseStatusCode == HttpStatusCode.OK ? address : null;
        }

        public async Task<Models.Address> GetAddressAsync(Guid addressId)
        {
            var response = await _client.ReadDocumentAsync(UriFactory.CreateDocumentUri(_databaseId, _collectionId, addressId.ToString()));

            await Task.FromResult(response);

            if (response.Resource == null)
                return null;

            var address = (Models.Address)(dynamic)response.Resource;

            return address;
        }
    }
}