namespace NCS.DSS.Address.GetAddressByIdHttpTrigger.Service
{
    public interface IGetAddressByIdHttpTriggerService
    {
        Task<Models.Address> GetAddressForCustomerAsync(Guid customerId, Guid addressId);
    }
}