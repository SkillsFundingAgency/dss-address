using System;
using System.Net;
using System.Threading.Tasks;
using NCS.DSS.Address.Cosmos.Provider;

namespace NCS.DSS.Address.PatchAddressHttpTrigger.Service
{
    public class PatchAddressHttpTriggerService : IPatchAddressHttpTriggerService
    {       
        public async Task<Models.Address> UpdateAsync(Models.Address address, Models.AddressPatch addressPatch)
        {
            if (address == null)
                return null;

            address.Patch(addressPatch);

            var documentDbProvider = new DocumentDBProvider();
            var response = await documentDbProvider.UpdateAddressAsync(address);

            var responseStatusCode = response.StatusCode;

            return responseStatusCode == HttpStatusCode.OK ? address : null;
        }

        public async Task<Models.Address> GetAddressAsync(Guid addressId)
        {
            var documentDbProvider = new DocumentDBProvider();
            var response = await documentDbProvider.GetAddressAsync(addressId);

            if (response?.Resource == null)
                return null;

            var address = (Models.Address)(dynamic)response.Resource;

            return address;
        }
    }
}