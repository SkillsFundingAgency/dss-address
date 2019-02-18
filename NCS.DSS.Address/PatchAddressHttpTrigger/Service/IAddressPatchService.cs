using NCS.DSS.Address.Models;

namespace NCS.DSS.Address.PatchAddressHttpTrigger.Service
{
    public interface IAddressPatchService
    {
        Models.Address Patch(string addressJson, AddressPatch addressPatch);
    }
}
