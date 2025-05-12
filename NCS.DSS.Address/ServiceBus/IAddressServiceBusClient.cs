namespace NCS.DSS.Address.ServiceBus
{
    public interface IAddressServiceBusClient
    {
        Task SendPatchMessageAsync(Models.Address address, Guid customerId, string reqUrl);
        Task SendPostMessageAsync(Models.Address address, string reqUrl);
    }
}