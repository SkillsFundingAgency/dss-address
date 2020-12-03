using DFC.JSON.Standard;
using NCS.DSS.Address.Models;
using Newtonsoft.Json.Linq;

namespace NCS.DSS.Address.PatchAddressHttpTrigger.Service
{
    public class AddressPatchService : IAddressPatchService
    {

        private readonly IJsonHelper _jsonHelper;

        public AddressPatchService(IJsonHelper jsonHelper)
        {
            _jsonHelper = jsonHelper;
        }

        public string Patch(string addressJson, AddressPatch addressPatch)
        {
            if (string.IsNullOrEmpty(addressJson))
                return null;

            var obj = JObject.Parse(addressJson);

            if (!string.IsNullOrEmpty(addressPatch.Address1))
                _jsonHelper.UpdatePropertyValue(obj["Address1"], addressPatch.Address1);

            if (addressPatch.Address2 != null)
                _jsonHelper.UpdatePropertyValue(obj["Address2"], addressPatch.Address2);

            if (addressPatch.Address3 != null)
                _jsonHelper.UpdatePropertyValue(obj["Address3"], addressPatch.Address3);

            if (addressPatch.Address4 != null)
                _jsonHelper.UpdatePropertyValue(obj["Address4"], addressPatch.Address4);

            if (addressPatch.Address5 != null)
                _jsonHelper.UpdatePropertyValue(obj["Address5"], addressPatch.Address5);

            if (!string.IsNullOrEmpty(addressPatch.PostCode))
                _jsonHelper.UpdatePropertyValue(obj["PostCode"], addressPatch.PostCode);

            if (addressPatch.AlternativePostCode != null)
                _jsonHelper.UpdatePropertyValue(obj["AlternativePostCode"], addressPatch.AlternativePostCode);

            if (addressPatch.Longitude.HasValue)
                _jsonHelper.UpdatePropertyValue(obj["Longitude"], addressPatch.Longitude);

            if (addressPatch.Latitude.HasValue)
                _jsonHelper.UpdatePropertyValue(obj["Latitude"], addressPatch.Latitude);

            if (addressPatch.EffectiveFrom.HasValue)
                _jsonHelper.UpdatePropertyValue(obj["EffectiveFrom"], addressPatch.EffectiveFrom);

            if (addressPatch.EffectiveTo.HasValue)
                _jsonHelper.UpdatePropertyValue(obj["EffectiveTo"], addressPatch.EffectiveTo);

            if (addressPatch.LastModifiedDate.HasValue)
                _jsonHelper.UpdatePropertyValue(obj["LastModifiedDate"], addressPatch.LastModifiedDate);

            if (!string.IsNullOrEmpty(addressPatch.LastModifiedTouchpointId))
                _jsonHelper.UpdatePropertyValue(obj["LastModifiedTouchpointId"], addressPatch.LastModifiedTouchpointId);

            if (!string.IsNullOrEmpty(addressPatch.SubcontractorId))
            {
                if (obj["SubcontractorId"] == null)
                    _jsonHelper.CreatePropertyOnJObject(obj, "SubcontractorId", addressPatch.SubcontractorId);
                else
                    _jsonHelper.UpdatePropertyValue(obj["SubcontractorId"], addressPatch.SubcontractorId);
            }

            return obj.ToString();

        }
    }
}