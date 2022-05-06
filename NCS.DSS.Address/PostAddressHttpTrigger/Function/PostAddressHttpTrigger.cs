using DFC.Common.Standard.Logging;
using DFC.GeoCoding.Standard.AzureMaps.Model;
using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using NCS.DSS.Address.Cosmos.Helper;
using NCS.DSS.Address.GeoCoding;
using NCS.DSS.Address.PostAddressHttpTrigger.Service;
using NCS.DSS.Address.Validation;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace NCS.DSS.Address.PostAddressHttpTrigger.Function
{
    public class PostAddressHttpTrigger
    {
        private IResourceHelper _resourceHelper;
        private IValidate _validate;
        private IPostAddressHttpTriggerService _addressPostService;
        private ILoggerHelper _loggerHelper;
        private IHttpRequestHelper _httpRequestHelper;
        private IHttpResponseMessageHelper _httpResponseMessageHelper;
        private IJsonHelper _jsonHelper;
        private IGeoCodingService _geoCodingService;

        public PostAddressHttpTrigger(IResourceHelper resourceHelper,
            IValidate validate,
            IPostAddressHttpTriggerService addressPostService,
            ILoggerHelper loggerHelper,
            IHttpRequestHelper httpRequestHelper,
            IHttpResponseMessageHelper httpResponseMessageHelper,
            IJsonHelper jsonHelper,
            IGeoCodingService geoCodingService)
        {
            _resourceHelper = resourceHelper;
            _validate = validate;
            _addressPostService = addressPostService;
            _loggerHelper = loggerHelper;
            _httpRequestHelper = httpRequestHelper;
            _httpResponseMessageHelper = httpResponseMessageHelper;
            _jsonHelper = jsonHelper;
            _geoCodingService = geoCodingService;
        }

        [FunctionName("Post")]
        [ProducesResponseType(typeof(Models.Address), 201)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Created, Description = "Address Created", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Address does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = 422, Description = "Address validation error(s)", ShowSchema = false)]
        [Display(Name = "Post", Description = "Ability to create a new address for a given customer")]
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Customers/{customerId}/Addresses")]HttpRequest req, ILogger log, string customerId)
        {
            _loggerHelper.LogMethodEnter(log);

            var correlationId = _httpRequestHelper.GetDssCorrelationId(req);
            if (string.IsNullOrEmpty(correlationId))
                log.LogInformation("Unable to locate 'DssCorrelationId; in request header");

            if (!Guid.TryParse(correlationId, out var correlationGuid))
            {
                log.LogInformation("Unable to Parse 'DssCorrelationId' to a Guid");
                correlationGuid = Guid.NewGuid();
            }

            var touchpointId = _httpRequestHelper.GetDssTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                _loggerHelper.LogInformationMessage(log, correlationGuid, "Unable to locate 'APIM-TouchpointId' in request header");
                return _httpResponseMessageHelper.BadRequest();
            }

            var subcontractorId = _httpRequestHelper.GetDssSubcontractorId(req);
            if (string.IsNullOrEmpty(subcontractorId))
            {
                log.LogInformation("Unable to locate 'SubcontractorId' in request header");
                return _httpResponseMessageHelper.BadRequest();
            }

            var ApimURL = _httpRequestHelper.GetDssApimUrl(req);
            if (string.IsNullOrEmpty(ApimURL))
            {
                log.LogInformation("Unable to locate 'apimurl' in request header");
                return _httpResponseMessageHelper.BadRequest();
            }

            var subContractorId = _httpRequestHelper.GetDssSubcontractorId(req);
            if (string.IsNullOrEmpty(subContractorId))
                _loggerHelper.LogInformationMessage(log, correlationGuid, "Unable to locate 'SubContractorId' in request header");

            _loggerHelper.LogInformationMessage(log, correlationGuid, "Post Address C# HTTP trigger function  processed a request. By Touchpoint " + touchpointId);

            if (!Guid.TryParse(customerId, out var customerGuid))
                return _httpResponseMessageHelper.BadRequest(customerGuid);

            Models.Address addressRequest;

            try
            {
                addressRequest = await _httpRequestHelper.GetResourceFromRequest<Models.Address>(req);
            }
            catch (JsonException ex)
            {
                return _httpResponseMessageHelper.UnprocessableEntity(ex);
            }

            if (addressRequest == null)
                return _httpResponseMessageHelper.UnprocessableEntity(req);

            addressRequest.SetIds(customerGuid, touchpointId, subContractorId);

            var errors = _validate.ValidateResource(addressRequest, true);

            if (errors != null && errors.Any())
                return _httpResponseMessageHelper.UnprocessableEntity(errors);

            _loggerHelper.LogInformationMessage(log, correlationGuid, "Attempting to get long and lat for postcode");
            Position position;

            try
            {
                var postcode = addressRequest.PostCode.Replace(" ", string.Empty);
                position = await _geoCodingService.GetPositionForPostcodeAsync(postcode);

            }
            catch (Exception e)
            {
                _loggerHelper.LogException(log, correlationGuid, string.Format("Unable to get long and lat for postcode: {0}", addressRequest.PostCode), e);
                throw;
            }

            addressRequest.SetLongitudeAndLatitude(position);

            var doesCustomerExist = await _resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
                return _httpResponseMessageHelper.NoContent(customerGuid);

            var isCustomerReadOnly = await _resourceHelper.IsCustomerReadOnly(customerGuid);

            if (isCustomerReadOnly)
                return _httpResponseMessageHelper.Forbidden(customerGuid);

            var address = await _addressPostService.CreateAsync(addressRequest);

            if (address != null)
                await _addressPostService.SendToServiceBusQueueAsync(address, ApimURL);

            _loggerHelper.LogMethodExit(log);

            return address == null
                ? _httpResponseMessageHelper.BadRequest(customerGuid) :
                _httpResponseMessageHelper.Created(_jsonHelper.SerializeObjectAndRenameIdProperty(address, "id", "AddressId"));
        }
    }
}