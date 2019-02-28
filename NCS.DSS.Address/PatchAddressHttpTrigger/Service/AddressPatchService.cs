using NCS.DSS.Address.Helpers;
using NCS.DSS.Address.Models;
using Newtonsoft.Json.Linq;

namespace NCS.DSS.Address.PatchAddressHttpTrigger.Service
{
    public class AddressPatchService : IAddressPatchService
    {
        public string Patch(string addressJson, AddressPatch addressPatch)
        {
            if (string.IsNullOrEmpty(addressJson))
                return null;

            var obj = JObject.Parse(addressJson);

            if (!string.IsNullOrEmpty(addressPatch.Address1))
                JsonHelper.UpdatePropertyValue(obj["Address1"], addressPatch.Address1);

            if (!string.IsNullOrEmpty(addressPatch.Address2))
                JsonHelper.UpdatePropertyValue(obj["Address2"], addressPatch.Address2);

            if (!string.IsNullOrEmpty(addressPatch.Address3))
                JsonHelper.UpdatePropertyValue(obj["Address3"], addressPatch.Address3);

            if (!string.IsNullOrEmpty(addressPatch.Address4))
                JsonHelper.UpdatePropertyValue(obj["Address4"], addressPatch.Address4);

            if (!string.IsNullOrEmpty(addressPatch.Address5))
                JsonHelper.UpdatePropertyValue(obj["Address5"], addressPatch.Address5);

            if (!string.IsNullOrEmpty(addressPatch.PostCode))
                JsonHelper.UpdatePropertyValue(obj["PostCode"], addressPatch.PostCode);

            if (!string.IsNullOrEmpty(addressPatch.AlternativePostCode))
                JsonHelper.UpdatePropertyValue(obj["AlternativePostCode"], addressPatch.AlternativePostCode);

            if (addressPatch.Longitude.HasValue)
                JsonHelper.UpdatePropertyValue(obj["Longitude"], addressPatch.Longitude);

            if (addressPatch.Latitude.HasValue)
                JsonHelper.UpdatePropertyValue(obj["Latitude"], addressPatch.Latitude);

            if (addressPatch.EffectiveFrom.HasValue)
                JsonHelper.UpdatePropertyValue(obj["EffectiveFrom"], addressPatch.EffectiveFrom);

            if (addressPatch.EffectiveTo.HasValue)
                JsonHelper.UpdatePropertyValue(obj["EffectiveTo"], addressPatch.EffectiveTo);

            if (addressPatch.LastModifiedDate.HasValue)
                JsonHelper.UpdatePropertyValue(obj["LastModifiedDate"], addressPatch.LastModifiedDate);

            if (!string.IsNullOrEmpty(addressPatch.LastModifiedTouchpointId))
                JsonHelper.UpdatePropertyValue(obj["LastModifiedTouchpointId"], addressPatch.LastModifiedTouchpointId);

            return obj.ToString();

        }
    }
}
