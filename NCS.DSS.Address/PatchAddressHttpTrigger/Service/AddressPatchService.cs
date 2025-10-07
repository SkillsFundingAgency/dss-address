using DFC.JSON.Standard;
using Microsoft.Extensions.Logging;
using NCS.DSS.Address.Models;
using Newtonsoft.Json.Linq;
namespace NCS.DSS.Address.PatchAddressHttpTrigger.Service
{
    public class AddressPatchService : IAddressPatchService
    {
        private readonly IJsonHelper _jsonHelper;
        private readonly ILogger<AddressPatchService> _logger;

        public AddressPatchService(IJsonHelper jsonHelper, ILogger<AddressPatchService> logger)
        {
            _jsonHelper = jsonHelper;
            _logger = logger;
        }

        public string Patch(string addressJson, AddressPatch addressPatch)
        {
            _logger.LogTrace("Started updating address json object with PATCH request");
            if (string.IsNullOrEmpty(addressJson))
            {
                _logger.LogInformation("Invalid addressJson object provided. diversity json is either empty or null");
                return null;
            }

            var obj = JObject.Parse(addressJson);

            if (!string.IsNullOrEmpty(addressPatch.Address1))
            {
                _jsonHelper.UpdatePropertyValue(obj["Address1"], addressPatch.Address1);
                _logger.LogTrace("Address1 Update Complete in Json Object");
            }

            if (addressPatch.Address2 != null)
            {
                _jsonHelper.UpdatePropertyValue(obj["Address2"], addressPatch.Address2);
                _logger.LogTrace("Address2 Update Complete in Json Object");
            }


            if (addressPatch.Address3 != null)
            {
                _jsonHelper.UpdatePropertyValue(obj["Address3"], addressPatch.Address3);
                _logger.LogTrace("Address3 Update Complete in Json Object");
            }

            if (addressPatch.Address4 != null)
            {
                _jsonHelper.UpdatePropertyValue(obj["Address4"], addressPatch.Address4);
                _logger.LogTrace("Address4 Update Complete in Json Object");
            }


            if (addressPatch.Address5 != null)
            {
                _jsonHelper.UpdatePropertyValue(obj["Address5"], addressPatch.Address5);
                _logger.LogTrace("Address5 Update Complete in Json Object");
            }

            if (!string.IsNullOrEmpty(addressPatch.PostCode))
            {
                _jsonHelper.UpdatePropertyValue(obj["PostCode"], addressPatch.PostCode);
                _logger.LogTrace("PostCode Update Complete in Json Object");

                _jsonHelper.UpdatePropertyValue(obj["Longitude"], addressPatch.Longitude);
                _logger.LogTrace("Longitude Update Complete in Json Object");

                _jsonHelper.UpdatePropertyValue(obj["Latitude"], addressPatch.Latitude);
                _logger.LogTrace("Latitude Update Complete in Json Object");
            }
            else
            {
                if (addressPatch.Longitude.HasValue)
                {
                    _jsonHelper.UpdatePropertyValue(obj["Longitude"], addressPatch.Longitude);
                    _logger.LogTrace("Longitude Update Complete in Json Object");
                }

                if (addressPatch.Latitude.HasValue)
                {
                    _jsonHelper.UpdatePropertyValue(obj["Latitude"], addressPatch.Latitude);
                    _logger.LogTrace("Latitude Update Complete in Json Object");
                }
            }

            if (addressPatch.AlternativePostCode != null)
            {
                _jsonHelper.UpdatePropertyValue(obj["AlternativePostCode"], addressPatch.AlternativePostCode);
                _logger.LogTrace("AlternativePostCode Update Complete in Json Object");
            }

            if (addressPatch.EffectiveFrom.HasValue)
            {
                _jsonHelper.UpdatePropertyValue(obj["EffectiveFrom"], addressPatch.EffectiveFrom);
                _logger.LogTrace("EffectiveFrom Update Complete in Json Object");
            }


            if (addressPatch.EffectiveTo.HasValue)
            {
                _jsonHelper.UpdatePropertyValue(obj["EffectiveTo"], addressPatch.EffectiveTo);
                _logger.LogTrace("EffectiveTo Update Complete in Json Object");
            }


            if (addressPatch.LastModifiedDate.HasValue)
            {
                _jsonHelper.UpdatePropertyValue(obj["LastModifiedDate"], addressPatch.LastModifiedDate);
                _logger.LogTrace("LastModifiedDate Update Complete in Json Object");
            }


            if (!string.IsNullOrEmpty(addressPatch.LastModifiedTouchpointId))
            {
                _jsonHelper.UpdatePropertyValue(obj["LastModifiedTouchpointId"], addressPatch.LastModifiedTouchpointId);
                _logger.LogTrace("LastModifiedTouchpointId Update Complete in Json Object");
            }

            if (!string.IsNullOrEmpty(addressPatch.SubcontractorId))
            {
                if (obj["SubcontractorId"] == null)
                    _jsonHelper.CreatePropertyOnJObject(obj, "SubcontractorId", addressPatch.SubcontractorId);
                else
                    _jsonHelper.UpdatePropertyValue(obj["SubcontractorId"], addressPatch.SubcontractorId);
                _logger.LogTrace("Added or Updated SubcontractorId in Json Object");
            }
            _logger.LogTrace("Completed updating address json object with PATCH request");
            return obj.ToString();

        }
    }
}