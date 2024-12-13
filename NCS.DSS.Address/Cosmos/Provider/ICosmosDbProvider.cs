using Microsoft.Azure.Cosmos;

namespace NCS.DSS.Address.Cosmos.Provider
{
    public interface ICosmosDbProvider
    {
        Task<bool> DoesCustomerResourceExist(Guid customerId);
        Task<bool> DoesCustomerHaveATerminationDate(Guid customerId);
        Task<string> GetAddressByIdForUpdateAsync(Guid customerId, Guid addressId);
        Task<Models.Address> GetAddressForCustomerAsync(Guid customerId, Guid addressId);
        Task<List<Models.Address>> GetAddressesForCustomerAsync(Guid customerId);
        Task<ItemResponse<Models.Address>> CreateAddressAsync(Models.Address address);
        Task<ItemResponse<Models.Address>> UpdateAddressAsync(string addressJson, Guid addressId);
    }
}