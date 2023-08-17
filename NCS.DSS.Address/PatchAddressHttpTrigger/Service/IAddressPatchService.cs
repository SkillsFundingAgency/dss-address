using Microsoft.Extensions.Logging;
using NCS.DSS.Address.Models;

namespace NCS.DSS.Address.PatchAddressHttpTrigger.Service
{
    public interface IAddressPatchService
    {
        string Patch(string addressJson, AddressPatch addressPatch, ILogger logger);
    }
}
