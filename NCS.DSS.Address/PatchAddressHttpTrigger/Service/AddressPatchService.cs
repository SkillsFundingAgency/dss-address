using DFC.JSON.Standard;
using Microsoft.Extensions.Logging;
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

        public string Patch(string addressJson, AddressPatch addressPatch, ILogger logger)
        {
            logger.LogInformation("Started updating address json object with PATCH request");
            if (string.IsNullOrEmpty(addressJson))
                return null;

            var obj = JObject.Parse(addressJson);

            if (!string.IsNullOrEmpty(addressPatch.Address1))
            {
                _jsonHelper.UpdatePropertyValue(obj["Address1"], addressPatch.Address1);
                logger.LogInformation("Address1 Update Complete in Json Object");
            }

            if (addressPatch.Address2 != null)
            {
                _jsonHelper.UpdatePropertyValue(obj["Address2"], addressPatch.Address2);
                logger.LogInformation("Address2 Update Complete in Json Object");
            }


            if (addressPatch.Address3 != null)
            {
                _jsonHelper.UpdatePropertyValue(obj["Address3"], addressPatch.Address3);
                logger.LogInformation("Address3 Update Complete in Json Object");
            }

            if (addressPatch.Address4 != null)
            {
                _jsonHelper.UpdatePropertyValue(obj["Address4"], addressPatch.Address4);
                logger.LogInformation("Address4 Update Complete in Json Object");
            }


            if (addressPatch.Address5 != null)
            {
                _jsonHelper.UpdatePropertyValue(obj["Address5"], addressPatch.Address5);
                logger.LogInformation("Address5 Update Complete in Json Object");
            }

            if (!string.IsNullOrEmpty(addressPatch.PostCode))
            {
                _jsonHelper.UpdatePropertyValue(obj["PostCode"], addressPatch.PostCode);
                logger.LogInformation("PostCode Update Complete in Json Object");
            }


            if (addressPatch.AlternativePostCode != null)
            {
                _jsonHelper.UpdatePropertyValue(obj["AlternativePostCode"], addressPatch.AlternativePostCode);
                logger.LogInformation("AlternativePostCode Update Complete in Json Object");
            }


            if (addressPatch.Longitude.HasValue)
            {
                _jsonHelper.UpdatePropertyValue(obj["Longitude"], addressPatch.Longitude);
                logger.LogInformation("Longitude Update Complete in Json Object");
            }


            if (addressPatch.Latitude.HasValue)
            {
                _jsonHelper.UpdatePropertyValue(obj["Latitude"], addressPatch.Latitude);
                logger.LogInformation("Latitude Update Complete in Json Object");
            }


            if (addressPatch.EffectiveFrom.HasValue)
            {
                _jsonHelper.UpdatePropertyValue(obj["EffectiveFrom"], addressPatch.EffectiveFrom);
                logger.LogInformation("EffectiveFrom Update Complete in Json Object");
            }


            if (addressPatch.EffectiveTo.HasValue)
            {
                _jsonHelper.UpdatePropertyValue(obj["EffectiveTo"], addressPatch.EffectiveTo);
                logger.LogInformation("EffectiveTo Update Complete in Json Object");
            }


            if (addressPatch.LastModifiedDate.HasValue)
            {
                _jsonHelper.UpdatePropertyValue(obj["LastModifiedDate"], addressPatch.LastModifiedDate);
                logger.LogInformation("LastModifiedDate Update Complete in Json Object");
            }


            if (!string.IsNullOrEmpty(addressPatch.LastModifiedTouchpointId))
            {
                _jsonHelper.UpdatePropertyValue(obj["LastModifiedTouchpointId"], addressPatch.LastModifiedTouchpointId);
                logger.LogInformation("LastModifiedTouchpointId Update Complete in Json Object");
            }

            if (!string.IsNullOrEmpty(addressPatch.SubcontractorId))
            {
                if (obj["SubcontractorId"] == null)
                    _jsonHelper.CreatePropertyOnJObject(obj, "SubcontractorId", addressPatch.SubcontractorId);
                else
                    _jsonHelper.UpdatePropertyValue(obj["SubcontractorId"], addressPatch.SubcontractorId);
                logger.LogInformation("Added or Updated SubcontractorId in Json Object");
            }
            logger.LogInformation("Completed updating address json object with PATCH request. returning object back to caller");
            return obj.ToString();

        }
    }
}