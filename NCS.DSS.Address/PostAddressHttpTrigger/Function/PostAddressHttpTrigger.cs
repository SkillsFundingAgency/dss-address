using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DFC.Common.Standard.Logging;
using DFC.Functions.DI.Standard.Attributes;
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

namespace NCS.DSS.Address.PostAddressHttpTrigger.Function
{
    public static class PostAddressHttpTrigger
    {
        [FunctionName("Post")]
        [ProducesResponseType(typeof(Models.Address), 201)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Created, Description = "Address Created", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Address does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = 422, Description = "Address validation error(s)", ShowSchema = false)]
        [Display(Name = "Post", Description = "Ability to create a new address for a given customer")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Customers/{customerId}/Addresses")]HttpRequest req, ILogger log, string customerId,
            [Inject]IResourceHelper resourceHelper,
            [Inject]IValidate validate,
            [Inject]IPostAddressHttpTriggerService addressPostService,
            [Inject]ILoggerHelper loggerHelper,
            [Inject]IHttpRequestHelper httpRequestHelper,
            [Inject]IHttpResponseMessageHelper httpResponseMessageHelper,
            [Inject]IJsonHelper jsonHelper,
            [Inject]IGeoCodingService geoCodingService)
        {
            loggerHelper.LogMethodEnter(log);

            var correlationId = httpRequestHelper.GetDssCorrelationId(req);
            if (string.IsNullOrEmpty(correlationId))
                log.LogInformation("Unable to locate 'DssCorrelationId; in request header");

            if (!Guid.TryParse(correlationId, out var correlationGuid))
            {
                log.LogInformation("Unable to Parse 'DssCorrelationId' to a Guid");
                correlationGuid = Guid.NewGuid();
            }

            var touchpointId = httpRequestHelper.GetDssTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                loggerHelper.LogInformationMessage(log, correlationGuid, "Unable to locate 'APIM-TouchpointId' in request header");
                return httpResponseMessageHelper.BadRequest();
            }

            var ApimURL = httpRequestHelper.GetDssApimUrl(req);
            if (string.IsNullOrEmpty(ApimURL))
            {
                log.LogInformation("Unable to locate 'apimurl' in request header");
                return httpResponseMessageHelper.BadRequest();
            }

            var subContractorId = httpRequestHelper.GetDssSubcontractorId(req);
            if (string.IsNullOrEmpty(subContractorId))
                loggerHelper.LogInformationMessage(log, correlationGuid, "Unable to locate 'SubContractorId' in request header");

            loggerHelper.LogInformationMessage(log, correlationGuid, "Post Address C# HTTP trigger function  processed a request. By Touchpoint " + touchpointId);

            if (!Guid.TryParse(customerId, out var customerGuid))
                return httpResponseMessageHelper.BadRequest(customerGuid);

            Models.Address addressRequest;

            try
            {
                addressRequest = await httpRequestHelper.GetResourceFromRequest<Models.Address>(req);
            }
            catch (JsonException ex)
            {
                return httpResponseMessageHelper.UnprocessableEntity(ex);
            }

            if (addressRequest == null)
                return httpResponseMessageHelper.UnprocessableEntity(req);

            addressRequest.SetIds(customerGuid, touchpointId, subContractorId);

            var errors = validate.ValidateResource(addressRequest, true);

            if (errors != null && errors.Any())
                return httpResponseMessageHelper.UnprocessableEntity(errors);

            loggerHelper.LogInformationMessage(log, correlationGuid, "Attempting to get long and lat for postcode");
            Position position;

            try
            {
                var postcode = addressRequest.PostCode.Replace(" ", string.Empty);
                position = await geoCodingService.GetPositionForPostcodeAsync(postcode);

            }
            catch (Exception e)
            {
                loggerHelper.LogException(log, correlationGuid, string.Format("Unable to get long and lat for postcode: {0}", addressRequest.PostCode), e);
                throw;
            }

            addressRequest.SetLongitudeAndLatitude(position);

            var doesCustomerExist = await resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
                return httpResponseMessageHelper.NoContent(customerGuid);

            var isCustomerReadOnly = await resourceHelper.IsCustomerReadOnly(customerGuid);

            if (isCustomerReadOnly)
                return httpResponseMessageHelper.Forbidden(customerGuid);

            var address = await addressPostService.CreateAsync(addressRequest);

            if (address != null)
                await addressPostService.SendToServiceBusQueueAsync(address, ApimURL);

            loggerHelper.LogMethodExit(log);

            return address == null
                ? httpResponseMessageHelper.BadRequest(customerGuid) :
                httpResponseMessageHelper.Created(jsonHelper.SerializeObjectAndRenameIdProperty(address, "id", "AddressId"));
        }
    }
}