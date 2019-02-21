using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NCS.DSS.Address.Cosmos.Helper;
using NCS.DSS.Address.Helpers;
using NCS.DSS.Address.PatchAddressHttpTrigger.Service;
using NCS.DSS.Address.Validation;
using Newtonsoft.Json;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;

namespace NCS.DSS.Address.Tests
{
    [TestFixture]
    public class PatchAddressHttpTriggerTests
    {
        private const string ValidCustomerId = "7E467BDB-213F-407A-B86A-1954053D3C24";
        private const string ValidAddressId = "1e1a555c-9633-4e12-ab28-09ed60d51cb3";
        private const string InValidId = "1111111-2222-3333-4444-555555555555";
        private ILogger _log;
        private HttpRequestMessage _request;
        private IResourceHelper _resourceHelper;
        private IValidate _validate;
        private IHttpRequestMessageHelper _httpRequestMessageHelper;
        private IPatchAddressHttpTriggerService _patchAddressHttpTriggerService;
        private Models.Address _address;
        private Models.AddressPatch _addressPatch;

        [SetUp]
        public void Setup()
        {
            _address = Substitute.For<Models.Address>();
            _addressPatch = Substitute.For<Models.AddressPatch>();

            _request = new HttpRequestMessage()
            {
                Content = new StringContent(string.Empty),
                RequestUri = 
                    new Uri($"http://localhost:7071/api/Customers/7E467BDB-213F-407A-B86A-1954053D3C24/Addressess/1e1a555c-9633-4e12-ab28-09ed60d51cb3")
            };

            _log = Substitute.For<ILogger>();
            _resourceHelper = Substitute.For<IResourceHelper>();
            _validate = Substitute.For<IValidate>();
            _httpRequestMessageHelper = Substitute.For<IHttpRequestMessageHelper>();
            _patchAddressHttpTriggerService = Substitute.For<IPatchAddressHttpTriggerService>();
            _httpRequestMessageHelper.GetTouchpointId(_request).Returns("0000000001");
            _httpRequestMessageHelper.GetApimURL(_request).Returns("http://localhost:7071/");
        }

        [Test]
        public async Task PatchAddressHttpTrigger_ReturnsStatusCodeBadRequest_WhenTouchpointIdIsNotProvided()
        {
            _httpRequestMessageHelper.GetTouchpointId(_request).Returns((string)null);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidAddressId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PatchAddressHttpTrigger_ReturnsStatusCodeBadRequest_WhenCustomerIdIsInvalid()
        {
            var result = await RunFunction(InValidId, ValidAddressId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PatchAddressHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenAddressHasFailedValidation()
        {
            _httpRequestMessageHelper.GetAddressFromRequest<Models.AddressPatch>(_request).Returns(Task.FromResult(_addressPatch).Result);

            var validationResults = new List<ValidationResult> { new ValidationResult("address Id is Required") };
            _validate.ValidateResource(Arg.Any<Models.AddressPatch>(), false).Returns(validationResults);

            var result = await RunFunction(ValidCustomerId, ValidAddressId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual((HttpStatusCode)422, result.StatusCode);
        }

        [Test]
        public async Task PatchAddressHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenAddressRequestIsInvalid()
        {
            _httpRequestMessageHelper.GetAddressFromRequest<Models.AddressPatch>(_request).Throws(new JsonException());

            var result = await RunFunction(ValidCustomerId, ValidAddressId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual((HttpStatusCode)422, result.StatusCode);
        }

        [Test]
        public async Task PatchAddressHttpTrigger_ReturnsStatusCodeNoContent_WhenCustomerDoesNotExist()
        {
            _httpRequestMessageHelper.GetAddressFromRequest<Models.AddressPatch>(_request).Returns(Task.FromResult(_addressPatch).Result);

            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(false);

            var result = await RunFunction(ValidCustomerId, ValidAddressId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Test]
        public async Task PatchAddressHttpTrigger_ReturnsStatusCodeNoContent_WhenAddressDoesNotExist()
        {
            _httpRequestMessageHelper.GetAddressFromRequest<Models.AddressPatch>(_request).Returns(Task.FromResult(_addressPatch).Result);

            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).ReturnsForAnyArgs(true);

            _patchAddressHttpTriggerService.GetAddressForCustomerAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(Task.FromResult<string>(null).Result);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidAddressId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Test]
        public async Task PatchAddressHttpTrigger_ReturnsStatusCodeBadRequest_WhenUnableToUpdateAddressRecord()
        {
            _httpRequestMessageHelper.GetAddressFromRequest<Models.AddressPatch>(_request).Returns(Task.FromResult(_addressPatch).Result);

            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).ReturnsForAnyArgs(true);

            _patchAddressHttpTriggerService.GetAddressForCustomerAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(Task.FromResult<string>(_address.ToString()).Result);

            _patchAddressHttpTriggerService.UpdateCosmosAsync(Arg.Any<string>(), Arg.Any<Guid>()).Returns(Task.FromResult<Models.Address>(null).Result);

            var result = await RunFunction(ValidCustomerId, ValidAddressId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PatchAddressHttpTrigger_ReturnsStatusCodeOK_WhenRequestIsNotValid()
        {
            _httpRequestMessageHelper.GetAddressFromRequest<Models.AddressPatch>(_request).Returns(Task.FromResult(_addressPatch).Result);

            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).ReturnsForAnyArgs(true);

            _patchAddressHttpTriggerService.GetAddressForCustomerAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(Task.FromResult(_address.ToString()).Result);

            _patchAddressHttpTriggerService.UpdateCosmosAsync(Arg.Any<string>(), Arg.Any<Guid>()).Returns(Task.FromResult<Models.Address>(null).Result);

            var result = await RunFunction(ValidCustomerId, ValidAddressId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task PatchAddressHttpTrigger_ReturnsStatusCodeOK_WhenRequestIsValid()
        {
            _httpRequestMessageHelper.GetAddressFromRequest<Models.AddressPatch>(_request).Returns(Task.FromResult(_addressPatch).Result);

            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).ReturnsForAnyArgs(true);

            _patchAddressHttpTriggerService.GetAddressForCustomerAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(Task.FromResult(_address.ToString()).Result);

            _patchAddressHttpTriggerService.UpdateCosmosAsync(Arg.Any<string>(), Arg.Any<Guid>()).Returns(Task.FromResult(_address).Result);

            var result = await RunFunction(ValidCustomerId, ValidAddressId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
        }

        private async Task<HttpResponseMessage> RunFunction(string customerId, string addressId)
        {
            return await PatchAddressHttpTrigger.Function.PatchAddressHttpTrigger.Run(
                _request, _log, customerId, addressId, _resourceHelper, _httpRequestMessageHelper, _validate, _patchAddressHttpTriggerService).ConfigureAwait(false);
        }

    }
}