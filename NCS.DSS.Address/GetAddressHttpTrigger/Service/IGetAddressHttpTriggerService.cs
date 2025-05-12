namespace NCS.DSS.Address.GetAddressHttpTrigger.Service
{
    public interface IGetAddressHttpTriggerService
    {
        Task<List<Models.Address>> GetAddressesAsync(Guid customerId);
    }
}