using DFC.Common.Standard.Logging;
using DFC.HTTP.Standard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NCS.DSS.Address.Cosmos.Helper;
using NCS.DSS.Address.GeoCoding;
using NCS.DSS.Address.Helpers;
using NCS.DSS.Address.PostAddressHttpTrigger.Service;
using NCS.DSS.Address.Validation;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading.Tasks;
using AddressFunction = NCS.DSS.Address.PostAddressHttpTrigger.Function;

namespace NCS.DSS.Address.Tests.FunctionTest
{
    [TestFixture]
    public class PostAddressHttpTriggerTests
    {
        private const string ValidCustomerId = "7E467BDB-213F-407A-B86A-1954053D3C24";
        private const string InValidId = "1111111-2222-3333-4444-555555555555";

        private HttpRequest _request;
        private Mock<IResourceHelper> _resourceHelper;
        private IValidate _validate;        
        private Mock<IHttpRequestHelper> _httpRequestHelper;
        private Mock<IPostAddressHttpTriggerService> _postAddressHttpTriggerService;
        private Models.Address _address;
        private Mock<IGeoCodingService> _geoCodingService;
        private AddressFunction.PostAddressHttpTrigger _function;
        private Mock<ILogger<AddressFunction.PostAddressHttpTrigger>> _logger;
        private Mock<IDynamicHelper> _dynamicHelper;

        [SetUp]
        public void Setup()
        {
            _address = new Models.Address();
            _address.PostCode = string.Empty;
            _request = new DefaultHttpContext().Request;
            _validate = new Validate();
            _resourceHelper = new Mock<IResourceHelper>();            
            _httpRequestHelper = new Mock<IHttpRequestHelper>();
            _geoCodingService = new Mock<IGeoCodingService>();
            _postAddressHttpTriggerService = new Mock<IPostAddressHttpTriggerService>();
            _logger = new Mock<ILogger<AddressFunction.PostAddressHttpTrigger>>();
            _dynamicHelper = new Mock<IDynamicHelper>();

            _function = new AddressFunction.PostAddressHttpTrigger(_resourceHelper.Object,
                _validate,
                _postAddressHttpTriggerService.Object,                
                _httpRequestHelper.Object,
                _geoCodingService.Object,
                _logger.Object,
                _dynamicHelper.Object
                );
        }

        [Test]
        public async Task PostAddressHttpTrigger_ReturnsStatusCodeBadRequest_WhenTouchpointIdIsNotProvided()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns((string)null);

            // Act
            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task PostAddressHttpTrigger_ReturnsStatusCodeBadRequest_WhenCustomerIdIsInvalid()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost:7071/");
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");

            // Act
            var result = await RunFunction(InValidId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task PostAddressHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenAddressHasFailedValidation()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost:7071/");
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<Models.Address>(_request)).Returns(Task.FromResult(_address));
            var val = new Mock<IValidate>();
            var validationResults = new List<ValidationResult> { new ValidationResult("address Id is Required") };
            val.Setup(x => x.ValidateResource(It.IsAny<Models.Address>(), true)).Returns(validationResults);
            _function = new AddressFunction.PostAddressHttpTrigger(_resourceHelper.Object,
               val.Object,
               _postAddressHttpTriggerService.Object,               
               _httpRequestHelper.Object,
               _geoCodingService.Object,
               _logger.Object,
               _dynamicHelper.Object);

            // Act
            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.That(result, Is.InstanceOf<UnprocessableEntityObjectResult>());
        }

        [Test]
        public async Task PostAddressHttpTrigger_ReturnsStatusCodeUnprocessableEntity_WhenAddressRequestIsInvalid()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost:7071/");
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<Models.Address>(_request)).Returns(Task.FromResult(_address));
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<Models.Address>(It.IsAny<HttpRequest>())).Throws(new JsonException());

            // Act
            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.That(result, Is.InstanceOf<UnprocessableEntityObjectResult>());
        }

        [Test]
        public async Task PostAddressHttpTrigger_ReturnsStatusCodeNoContent_WhenCustomerDoesNotExist()
        {
            // Arrange
            _address.Address1 = "Some Address";
            _address.PostCode = "NE99 5EB";
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost:7071/");
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<Models.Address>(_request)).Returns(Task.FromResult(_address));
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(false));

            // Act
            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task PostAddressHttpTrigger_ReturnsStatusCodeBadRequest_WhenUnableToCreateAddressRecord()
        {
            // Arrange
            _address.Address1 = "Some Address";
            _address.PostCode = "NE99 5EB";
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost:7071/");
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<Models.Address>(_request)).Returns(Task.FromResult(_address));
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _postAddressHttpTriggerService.Setup(x => x.CreateAsync(It.IsAny<Models.Address>(), _logger.Object)).Returns(Task.FromResult<Models.Address>(null));

            // Act
            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task PostAddressHttpTrigger_ReturnsStatusCodeCreated_WhenPostcodeRequiresTrimming()
        {
            // Arrange
            _address.Address1 = "Some Address";
            _address.PostCode = "NE99 5EB ";
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost:7071/");
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<Models.Address>(_request)).Returns(Task.FromResult(_address));
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _postAddressHttpTriggerService.Setup(x => x.CreateAsync(It.IsAny<Models.Address>(), _logger.Object)).Returns(Task.FromResult<Models.Address>(_address));

            // Act
            var result = await RunFunction(ValidCustomerId);

            // Assert            
            Assert.That(result, Is.InstanceOf<JsonResult>());

            var responseResult = result as JsonResult;
            Assert.That(responseResult.StatusCode, Is.EqualTo((int)HttpStatusCode.Created));
        }

        [Test]
        public async Task PostAddressHttpTrigger_ReturnsStatusCodeCreated_WhenRequestIsValid()
        {
            // Arrange
            _address.Address1 = "Some Address";
            _address.PostCode = "NE99 5EB";
            _httpRequestHelper.Setup(x => x.GetDssApimUrl(_request)).Returns("http://localhost:7071/");
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _httpRequestHelper.Setup(x => x.GetResourceFromRequest<Models.Address>(_request)).Returns(Task.FromResult(_address));
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _postAddressHttpTriggerService.Setup(x => x.CreateAsync(It.IsAny<Models.Address>(), _logger.Object)).Returns(Task.FromResult<Models.Address>(_address));

            // Act
            var result = await RunFunction(ValidCustomerId);

            // Assert            
            Assert.That(result, Is.InstanceOf<JsonResult>());

            var responseResult = result as JsonResult;
            Assert.That(responseResult.StatusCode, Is.EqualTo((int)HttpStatusCode.Created));
        }

        private async Task<IActionResult> RunFunction(string customerId)
        {
            return await _function.Run(
                _request,
                customerId).ConfigureAwait(false);
        }
    }
}