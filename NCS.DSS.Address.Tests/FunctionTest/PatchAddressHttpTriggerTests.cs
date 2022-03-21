using DFC.Common.Standard.Logging;
using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Logging;
using Moq;
using NCS.DSS.Address.Cosmos.Helper;
using NCS.DSS.Address.GeoCoding;
using NCS.DSS.Address.PatchAddressHttpTrigger.Service;
using NCS.DSS.Address.Validation;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace NCS.DSS.Address.Tests.FunctionTest
{
    [TestFixture]
    public class PatchAddressHttpTriggerTests
    {
        private const string ValidCustomerId = "7E467BDB-213F-407A-B86A-1954053D3C24";
        private const string ValidAddressId = "1e1a555c-9633-4e12-ab28-09ed60d51cb3";
        private const string InValidId = "1111111-2222-3333-4444-555555555555";
        private Mock<ILogger> _log;
        private HttpRequest _request;
        private Mock<IResourceHelper> _resourceHelper;
        private IValidate _validate;
        private Mock<IPatchAddressHttpTriggerService> _patchAddressHttpTriggerService;
        private Mock<ILoggerHelper> _loggerHelper;
        private Mock<IHttpRequestHelper> _httpRequestHelper;
        private IHttpResponseMessageHelper _httpResponseMessageHelper;
        private IJsonHelper _jsonHelper;
        private Mock<IGeoCodingService> _geoCodingService;
        private PatchAddressHttpTrigger.Function.PatchAddressHttpTrigger _function;
        private Models.Address _address;
        private Models.AddressPatch _addressPatch;
        private string _addressString;

        [SetUp]
        public void Setup()
        {
            _address = new Models.Address();
            _addressPatch = new Models.AddressPatch();

            _request = new DefaultHttpRequest(new DefaultHttpContext());

            _log = new Mock<ILogger>();
            _resourceHelper = new Mock<IResourceHelper>();
            _validate = new Validate();
            _httpRequestHelper = new Mock<IHttpRequestHelper>();
            _loggerHelper = new Mock<ILoggerHelper>();
            _httpRequestHelper = new Mock<IHttpRequestHelper>();
            _httpResponseMessageHelper = new HttpResponseMessageHelper();
            _jsonHelper = new JsonHelper();
            _geoCodingService = new Mock<IGeoCodingService>();
            _addressString = JsonConvert.SerializeObject(_address);
            _patchAddressHttpTriggerService = new Mock<IPatchAddressHttpTriggerService>();
            _function = new PatchAddressHttpTrigger.Function.PatchAddressHttpTrigger(_resourceHelper.Object,
                _validate,
                _patchAddressHttpTriggerService.Object,
                _loggerHelper.Object,
                _httpRequestHelper.Object,
                _httpResponseMessageHelper,
                _jsonHelper,
                _geoCodingService.Object);
        }

        [Test]
        public async Task PatchAddressHttpTrigger_ReturnsStatusCodeBadRequest_WhenTouchpointIdIsNotProvided()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns((string)null);
            _httpRequestHelper.Setup(x => x.GetDssSubcontractorId(_request)).Returns("9999999999");

            // Act
            var result = await RunFunction(ValidCustomerId, ValidAddressId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PatchAddressHttpTrigger_ReturnsStatusCodeBadRequest_WhenSubscriberIdIsNotProvided()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssSubcontractorId(_request)).Returns((string)null);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidAddressId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PatchAddressHttpTrigger_ReturnsStatusCodeBadRequest_WhenCustomerIdIsInvalid()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssSubcontractorId(_request)).Returns("9999999999");


            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost:7071/");

            // Act
            var result = await RunFunction(InValidId, ValidAddressId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PatchAddressHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenAddressHasFailedValidation()
        {
            // Arrange
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<Models.AddressPatch>(_request)).Returns(Task.FromResult(_addressPatch));
            var val = new Mock<IValidate>();
            var validationResults = new List<ValidationResult> { new ValidationResult("address Id is Required") };
            val.Setup(x=>x.ValidateResource(It.IsAny<Models.AddressPatch>(), false)).Returns(validationResults);
            _function = new PatchAddressHttpTrigger.Function.PatchAddressHttpTrigger(_resourceHelper.Object,
                val.Object,
                _patchAddressHttpTriggerService.Object,
                _loggerHelper.Object,
                _httpRequestHelper.Object,
                _httpResponseMessageHelper,
                _jsonHelper,
                _geoCodingService.Object);
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssSubcontractorId(_request)).Returns("9999999999");
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost:7071/");

            // Act
            var result = await RunFunction(ValidCustomerId, ValidAddressId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual((HttpStatusCode)422, result.StatusCode);
        }

        [Test]
        public async Task PatchAddressHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenAddressRequestIsInvalid()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssSubcontractorId(_request)).Returns("9999999999");
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost:7071/");
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<Models.AddressPatch>(_request)).Throws(new JsonException());

            // Act
            var result = await RunFunction(ValidCustomerId, ValidAddressId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual((HttpStatusCode)422, result.StatusCode);
        }

        [Test]
        public async Task PatchAddressHttpTrigger_ReturnsStatusCodeNoContent_WhenCustomerDoesNotExist()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssSubcontractorId(_request)).Returns("9999999999");
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost:7071/");
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(false));
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<Models.AddressPatch>(_request)).Returns(Task.FromResult(_addressPatch));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidAddressId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Test]
        public async Task PatchAddressHttpTrigger_ReturnsStatusCodeNoContent_WhenAddressDoesNotExist()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssSubcontractorId(_request)).Returns("9999999999");
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost:7071/");
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<Models.AddressPatch>(_request)).Returns(Task.FromResult(_addressPatch));
            _patchAddressHttpTriggerService.Setup(x => x.GetAddressForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult<string>(null));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidAddressId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Test]
        public async Task PatchAddressHttpTrigger_ReturnsStatusCodeBadRequest_WhenUnableToUpdateAddressRecord()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssSubcontractorId(_request)).Returns("9999999999");
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost:7071/");
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<Models.AddressPatch>(_request)).Returns(Task.FromResult(_addressPatch));
            _patchAddressHttpTriggerService.Setup(x => x.UpdateCosmosAsync(It.IsAny<string>(), It.IsAny<Guid>())).Returns(Task.FromResult<Models.Address>(null));
            _patchAddressHttpTriggerService.Setup(x => x.GetAddressForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult<string>(_addressString));
            _patchAddressHttpTriggerService.Setup(x => x.PatchResource(It.IsAny<string>(), It.IsAny<Models.AddressPatch>())).Returns(_addressString);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidAddressId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PatchAddressHttpTrigger_ReturnsStatusCodeOK_WhenRequestIsValid()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssSubcontractorId(_request)).Returns("9999999999");
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost:7071/");
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<Models.AddressPatch>(_request)).Returns(Task.FromResult(_addressPatch));
            _patchAddressHttpTriggerService.Setup(x => x.UpdateCosmosAsync(It.IsAny<string>(), It.IsAny<Guid>())).Returns(Task.FromResult<Models.Address>(_address));
            _patchAddressHttpTriggerService.Setup(x => x.GetAddressForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult<string>(_addressString));
            _patchAddressHttpTriggerService.Setup(x => x.PatchResource(It.IsAny<string>(), It.IsAny<Models.AddressPatch>())).Returns(_addressString);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidAddressId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
        }

        private async Task<HttpResponseMessage> RunFunction(string customerId, string addressId)
        {
            return await _function.Run(
                _request,
                _log.Object,
                customerId,
                addressId).ConfigureAwait(false);
        }

    }
}