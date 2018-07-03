using System;
using System.Net;
using System.Threading.Tasks;
using NCS.DSS.Address.Cosmos.Provider;

namespace NCS.DSS.Address.PostAddressHttpTrigger.Service
{
    public class PostAddressHttpTriggerService : IPostAddressHttpTriggerService
    {
        public async Task<Guid?> CreateAsync(Models.Address address)
        {
            if (address == null)
                return null;
          
            var addressId = Guid.NewGuid();
            address.AddressId = addressId;

            var documentDbProvider = new DocumentDBProvider();

            var response = await documentDbProvider.CreateAddressAsync(address);

            return response.StatusCode == HttpStatusCode.Created ? addressId : (Guid?) null;
        }
    }
}