using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace NCS.DSS.Address.Cosmos.Provider
{
    public interface IDocumentDBProvider
    {
        bool DoesCustomerResourceExist(Guid customerId);
        Task<ResourceResponse<Document>> GetAddressAsync(Guid addressId);
        List<Models.Address> GetAddressesForCustomer(Guid customerId);
        Task<ResourceResponse<Document>> CreateAddressAsync(Models.Address address);
        Task<ResourceResponse<Document>> UpdateAddressAsync(Models.Address address);
    }
}