using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Logging;

namespace NCS.DSS.Address.Cosmos.Provider
{
    public interface IDocumentDBProvider
    {
        Task<bool> DoesCustomerResourceExist(Guid customerId);
        Task<bool> DoesCustomerResourceExist(Guid customerId, ILogger logger);
        Task<bool> DoesCustomerHaveATerminationDate(Guid customerId);
        Task<string> GetAddressByIdForUpdateAsync(Guid customerId, Guid addressId);
        Task<Models.Address> GetAddressForCustomerAsync(Guid customerId, Guid addressId);
        Task<List<Models.Address>> GetAddressesForCustomerAsync(Guid customerId);
        Task<ResourceResponse<Document>> CreateAddressAsync(Models.Address address);
        Task<ResourceResponse<Document>> UpdateAddressAsync(string addressJson, Guid addressId);
    }
}