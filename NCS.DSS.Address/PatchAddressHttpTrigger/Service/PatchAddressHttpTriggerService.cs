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

            if (!addressPatch.LastModifiedDate.HasValue)
                addressPatch.LastModifiedDate = DateTime.Now;

            address.Patch(addressPatch);

            var documentDbProvider = new DocumentDBProvider();
            var response = await documentDbProvider.UpdateAddressAsync(address);

            var responseStatusCode = response.StatusCode;

            return responseStatusCode == HttpStatusCode.OK ? address : null;
        }

        public async Task<Models.Address> GetAddressForCustomerAsync(Guid customerId, Guid addressId)
        {
            var documentDbProvider = new DocumentDBProvider();
            var address = await documentDbProvider.GetAddressForCustomerAsync(customerId, addressId);
            
            return address;
        }
    }
}