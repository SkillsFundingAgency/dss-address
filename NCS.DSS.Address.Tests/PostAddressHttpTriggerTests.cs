using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NCS.DSS.Address.Cosmos.Helper;
using NCS.DSS.Address.Helpers;
using NCS.DSS.Address.PostAddressHttpTrigger.Service;
using NCS.DSS.Address.Validation;
using NSubstitute;
using NUnit.Framework;

namespace NCS.DSS.Address.Tests
{
    [TestFixture]
    public class PostAddressHttpTriggerTests
    {
        private const string ValidCustomerId = "7E467BDB-213F-407A-B86A-1954053D3C24";
        private const string InValidId = "1111111-2222-3333-4444-555555555555";
        private ILogger _log;
        private HttpRequestMessage _request;
        private IResourceHelper _resourceHelper;
        private IValidate _validate;
        private IHttpRequestMessageHelper _httpRequestMessageHelper;
        private IPostAddressHttpTriggerService _postAddressHttpTriggerService;
        private Models.Address _address;

        [SetUp]
        public void Setup()
        {
            _address = Substitute.For<Models.Address>();

            _request = new HttpRequestMessage()
            {
                Content = new StringContent(string.Empty),
                RequestUri = 
                    new Uri($"http://localhost:7071/api/Customers/7E467BDB-213F-407A-B86A-1954053D3C24/Addressess/")
            };

            _log = Substitute.For<ILogger>();
            _resourceHelper = Substitute.For<IResourceHelper>();
            _httpRequestMessageHelper = Substitute.For<IHttpRequestMessageHelper>();
            _validate = Substitute.For<IValidate>();
            _postAddressHttpTriggerService = Substitute.For<IPostAddressHttpTriggerService>();
        }

        [Test]
        public async Task PostAddressHttpTrigger_ReturnsStatusCodeBadRequest_WhenCustomerIdIsInvalid()
        {
            var result = await RunFunction(InValidId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PostAddressHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenAddressHasFailedValidation()
        {
            _httpRequestMessageHelper.GetAddressFromRequest<Models.Address>(_request).Returns(Task.FromResult(_address).Result);

            var validationResults = new List<ValidationResult> { new ValidationResult("address Id is Required") };
            _validate.ValidateResource(Arg.Any<Models.Address>()).Returns(validationResults);

            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual((HttpStatusCode)422, result.StatusCode);
        }

        [Test]
        public async Task PostAddressHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenAddressRequestIsInvalid()
        {
            var validationResults = new List<ValidationResult> { new ValidationResult("Customer Id is Required") };
            _validate.ValidateResource(Arg.Any<Models.Address>()).Returns(validationResults);

            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual((HttpStatusCode)422, result.StatusCode);
        }

        [Test]
        public async Task PostAddressHttpTrigger_ReturnsStatusCodeNoContent_WhenCustomerDoesNotExist()
        {
            _httpRequestMessageHelper.GetAddressFromRequest<Models.Address>(_request).Returns(Task.FromResult(_address).Result);

            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(false);

            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Test]
        public async Task PostAddressHttpTrigger_ReturnsStatusCodeBadRequest_WhenUnableToCreateAddressRecord()
        {
            _httpRequestMessageHelper.GetAddressFromRequest<Models.Address>(_request).Returns(Task.FromResult(_address).Result);

            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).ReturnsForAnyArgs(true);

            _postAddressHttpTriggerService.CreateAsync(Arg.Any<Models.Address>()).Returns(Task.FromResult<Models.Address>(null).Result);

            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PostAddressHttpTrigger_ReturnsStatusCodeCreated_WhenRequestNotIsValid()
        {
            _httpRequestMessageHelper.GetAddressFromRequest<Models.Address>(_request).Returns(Task.FromResult(_address).Result);

            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).ReturnsForAnyArgs(true);

            _postAddressHttpTriggerService.CreateAsync(Arg.Any<Models.Address>()).Returns(Task.FromResult<Models.Address>(null).Result);

            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PostAddressHttpTrigger_ReturnsStatusCodeCreated_WhenRequestIsValid()
        {
            _httpRequestMessageHelper.GetAddressFromRequest<Models.Address>(_request).Returns(Task.FromResult(_address).Result);

            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).ReturnsForAnyArgs(true);

            _postAddressHttpTriggerService.CreateAsync(Arg.Any<Models.Address>()).Returns(Task.FromResult(_address).Result);

            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.Created, result.StatusCode);
        }

        private async Task<HttpResponseMessage> RunFunction(string customerId)
        {
            return await PostAddressHttpTrigger.Function.PostAddressHttpTrigger.Run(
                _request, _log, customerId, _resourceHelper, _httpRequestMessageHelper, _validate, _postAddressHttpTriggerService).ConfigureAwait(false);
        }

    }
}