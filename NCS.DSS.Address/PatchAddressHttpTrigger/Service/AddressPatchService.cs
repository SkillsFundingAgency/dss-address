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
            _logger.LogInformation("Started updating address json object with PATCH request");
            if (string.IsNullOrEmpty(addressJson))
            {
                _logger.LogWarning("Invalid addressJson object provided. diversity json is either empty or null");
                return null;
            }

            var obj = JObject.Parse(addressJson);

            if (!string.IsNullOrEmpty(addressPatch.Address1))
            {
                _jsonHelper.UpdatePropertyValue(obj["Address1"], addressPatch.Address1);
                _logger.LogInformation("Address1 Update Complete in Json Object");
            }

            if (addressPatch.Address2 != null)
            {
                _jsonHelper.UpdatePropertyValue(obj["Address2"], addressPatch.Address2);
                _logger.LogInformation("Address2 Update Complete in Json Object");
            }


            if (addressPatch.Address3 != null)
            {
                _jsonHelper.UpdatePropertyValue(obj["Address3"], addressPatch.Address3);
                _logger.LogInformation("Address3 Update Complete in Json Object");
            }

            if (addressPatch.Address4 != null)
            {
                _jsonHelper.UpdatePropertyValue(obj["Address4"], addressPatch.Address4);
                _logger.LogInformation("Address4 Update Complete in Json Object");
            }


            if (addressPatch.Address5 != null)
            {
                _jsonHelper.UpdatePropertyValue(obj["Address5"], addressPatch.Address5);
                _logger.LogInformation("Address5 Update Complete in Json Object");
            }

            if (!string.IsNullOrEmpty(addressPatch.PostCode))
            {
                _jsonHelper.UpdatePropertyValue(obj["PostCode"], addressPatch.PostCode);
                _logger.LogInformation("PostCode Update Complete in Json Object");

                _jsonHelper.UpdatePropertyValue(obj["Longitude"], addressPatch.Longitude);
                _logger.LogInformation("Longitude Update Complete in Json Object");

                _jsonHelper.UpdatePropertyValue(obj["Latitude"], addressPatch.Latitude);
                _logger.LogInformation("Latitude Update Complete in Json Object");
            }
            else
            {
                if (addressPatch.Longitude.HasValue)
                {
                    _jsonHelper.UpdatePropertyValue(obj["Longitude"], addressPatch.Longitude);
                    _logger.LogInformation("Longitude Update Complete in Json Object");
                }

                if (addressPatch.Latitude.HasValue)
                {
                    _jsonHelper.UpdatePropertyValue(obj["Latitude"], addressPatch.Latitude);
                    _logger.LogInformation("Latitude Update Complete in Json Object");
                }
            }

            if (addressPatch.AlternativePostCode != null)
            {
                _jsonHelper.UpdatePropertyValue(obj["AlternativePostCode"], addressPatch.AlternativePostCode);
                _logger.LogInformation("AlternativePostCode Update Complete in Json Object");
            }

            if (addressPatch.EffectiveFrom.HasValue)
            {
                _jsonHelper.UpdatePropertyValue(obj["EffectiveFrom"], addressPatch.EffectiveFrom);
                _logger.LogInformation("EffectiveFrom Update Complete in Json Object");
            }


            if (addressPatch.EffectiveTo.HasValue)
            {
                _jsonHelper.UpdatePropertyValue(obj["EffectiveTo"], addressPatch.EffectiveTo);
                _logger.LogInformation("EffectiveTo Update Complete in Json Object");
            }


            if (addressPatch.LastModifiedDate.HasValue)
            {
                _jsonHelper.UpdatePropertyValue(obj["LastModifiedDate"], addressPatch.LastModifiedDate);
                _logger.LogInformation("LastModifiedDate Update Complete in Json Object");
            }


            if (!string.IsNullOrEmpty(addressPatch.LastModifiedTouchpointId))
            {
                _jsonHelper.UpdatePropertyValue(obj["LastModifiedTouchpointId"], addressPatch.LastModifiedTouchpointId);
                _logger.LogInformation("LastModifiedTouchpointId Update Complete in Json Object");
            }

            if (!string.IsNullOrEmpty(addressPatch.SubcontractorId))
            {
                if (obj["SubcontractorId"] == null)
                    _jsonHelper.CreatePropertyOnJObject(obj, "SubcontractorId", addressPatch.SubcontractorId);
                else
                    _jsonHelper.UpdatePropertyValue(obj["SubcontractorId"], addressPatch.SubcontractorId);
                _logger.LogInformation("Added or Updated SubcontractorId in Json Object");
            }
            _logger.LogInformation("Completed updating address json object with PATCH request");
            return obj.ToString();

        }
    }
}