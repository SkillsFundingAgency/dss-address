using DFC.Common.Standard.Logging;
using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Logging;
using Moq;
using NCS.DSS.Address.Cosmos.Helper;
using NCS.DSS.Address.GeoCoding;
using NCS.DSS.Address.PostAddressHttpTrigger.Service;
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
    public class PostAddressHttpTriggerTests
    {
        private const string ValidCustomerId = "7E467BDB-213F-407A-B86A-1954053D3C24";
        private const string InValidId = "1111111-2222-3333-4444-555555555555";
        private Mock<ILogger> _log;
        private HttpRequest _request;
        private Mock<IResourceHelper> _resourceHelper;
        private IValidate _validate;
        private Mock<ILoggerHelper> _loggerHelper;
        private Mock<IHttpRequestHelper> _httpRequestHelper;
        private IHttpResponseMessageHelper _httpResponseMessageHelper;
        private IJsonHelper _jsonHelper;
        private Mock<IPostAddressHttpTriggerService> _postAddressHttpTriggerService;
        private Models.Address _address;
        private Mock<IGeoCodingService> _geoCodingService;
        private PostAddressHttpTrigger.Function.PostAddressHttpTrigger _function;

        [SetUp]
        public void Setup()
        {
            _address = new Models.Address();
            _address.PostCode = string.Empty;
            _request = new DefaultHttpRequest(new DefaultHttpContext());
            _log = new Mock<ILogger>();
            _validate = new Validate();
            _resourceHelper = new Mock<IResourceHelper>();
            _loggerHelper = new Mock<ILoggerHelper>();
            _httpRequestHelper = new Mock<IHttpRequestHelper>();
            _httpResponseMessageHelper = new HttpResponseMessageHelper();
            _jsonHelper = new JsonHelper();
            _log = new Mock<ILogger>();
            _geoCodingService = new Mock<IGeoCodingService>();
            _postAddressHttpTriggerService = new Mock<IPostAddressHttpTriggerService>();

            _function = new PostAddressHttpTrigger.Function.PostAddressHttpTrigger(_resourceHelper.Object, 
                _validate, 
                _postAddressHttpTriggerService.Object, 
                _loggerHelper.Object,
                _httpRequestHelper.Object, 
                _httpResponseMessageHelper, 
                _jsonHelper, 
                _geoCodingService.Object);
        }

        [Test]
        public async Task PostAddressHttpTrigger_ReturnsStatusCodeBadRequest_WhenTouchpointIdIsNotProvided()
        {
            // Arrange
            _httpRequestHelper.Setup(x=>x.GetDssTouchpointId(_request)).Returns((string)null);
            _httpRequestHelper.Setup(x => x.GetDssSubcontractorId(_request)).Returns("9999999999");


            // Act
            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PostAddressHttpTrigger_ReturnsStatusCodeBadRequest_WhenSubcontractorIdIsNotProvided()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssSubcontractorId(_request)).Returns((string)null);

            // Act
            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PostAddressHttpTrigger_ReturnsStatusCodeBadRequest_WhenCustomerIdIsInvalid()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost:7071/");
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssSubcontractorId(_request)).Returns("9999999999");

            // Act
            var result = await RunFunction(InValidId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PostAddressHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenAddressHasFailedValidation()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost:7071/");
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssSubcontractorId(_request)).Returns("9999999999");
            _httpRequestHelper.Setup(x=>x.GetResourceFromRequest<Models.Address>(_request)).Returns(Task.FromResult(_address));
            var val = new Mock<IValidate>();
            var validationResults = new List<ValidationResult> { new ValidationResult("address Id is Required") };
            val.Setup(x=>x.ValidateResource(It.IsAny<Models.Address>(), true)).Returns(validationResults);
            _function = new PostAddressHttpTrigger.Function.PostAddressHttpTrigger(_resourceHelper.Object,
               val.Object,
               _postAddressHttpTriggerService.Object,
               _loggerHelper.Object,
               _httpRequestHelper.Object,
               _httpResponseMessageHelper,
               _jsonHelper,
               _geoCodingService.Object);

            // Act
            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual((HttpStatusCode)422, result.StatusCode);
        }

        [Test]
        public async Task PostAddressHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenAddressRequestIsInvalid()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost:7071/");
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssSubcontractorId(_request)).Returns("9999999999");
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<Models.Address>(_request)).Returns(Task.FromResult(_address));
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<Models.Address>(It.IsAny<HttpRequest>())).Throws(new JsonException());

            // Act
            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual((HttpStatusCode)422, result.StatusCode);
        }

        [Test]
        public async Task PostAddressHttpTrigger_ReturnsStatusCodeNoContent_WhenCustomerDoesNotExist()
        {
            // Arrange
            _address.Address1 = "Some Address";
            _address.PostCode = "NE99 5EB";
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost:7071/");
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssSubcontractorId(_request)).Returns("9999999999");
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<Models.Address>(_request)).Returns(Task.FromResult(_address));
            _resourceHelper.Setup(x=>x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(false));

            // Act
            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Test]
        public async Task PostAddressHttpTrigger_ReturnsStatusCodeBadRequest_WhenUnableToCreateAddressRecord()
        {
            // Arrange
            _address.Address1 = "Some Address";
            _address.PostCode = "NE99 5EB";
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost:7071/");
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssSubcontractorId(_request)).Returns("9999999999");
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<Models.Address>(_request)).Returns(Task.FromResult(_address));
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _postAddressHttpTriggerService.Setup(x=>x.CreateAsync(It.IsAny<Models.Address>())).Returns(Task.FromResult<Models.Address>(null));

            // Act
            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PostAddressHttpTrigger_ReturnsStatusCodeCreated_WhenRequestIsValid()
        {
            // Arrange
            _address.Address1 = "Some Address";
            _address.PostCode = "NE99 5EB";
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost:7071/");
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetDssSubcontractorId(_request)).Returns("9999999999");
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<Models.Address>(_request)).Returns(Task.FromResult(_address));
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _postAddressHttpTriggerService.Setup(x => x.CreateAsync(It.IsAny<Models.Address>())).Returns(Task.FromResult<Models.Address>(_address));

            // Act
            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.Created, result.StatusCode);
        }

        private async Task<HttpResponseMessage> RunFunction(string customerId)
        {
            return await _function.Run(
                _request,
                _log.Object,
                customerId).ConfigureAwait(false);
        }
    }
}