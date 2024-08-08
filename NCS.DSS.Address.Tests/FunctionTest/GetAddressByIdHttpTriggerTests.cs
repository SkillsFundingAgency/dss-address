using DFC.Common.Standard.Logging;
using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NCS.DSS.Address.Cosmos.Helper;
using NCS.DSS.Address.GetAddressByIdHttpTrigger.Service;
using NUnit.Framework;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AddressFunction = NCS.DSS.Address.GetAddressByIdHttpTrigger.Function;

namespace NCS.DSS.Address.Tests.FunctionTest
{
    [TestFixture]
    public class GetAddressByIdHttpTriggerTests
    {
        private const string ValidCustomerId = "7E467BDB-213F-407A-B86A-1954053D3C24";
        private const string ValidAddressId = "1e1a555c-9633-4e12-ab28-09ed60d51cb3";
        private const string InValidId = "1111111-2222-3333-4444-555555555555";
        private Mock<HttpRequest> _request;        
        private Mock<IResourceHelper> _resourceHelper;        
        private Mock<IHttpRequestHelper> _httpRequestHelper;        
        private Mock<IGetAddressByIdHttpTriggerService> _getAddressByIdHttpTriggerService;
        private Models.Address _address;
        private AddressFunction.GetAddressByIdHttpTrigger _function;
        private Mock<ILogger<AddressFunction.GetAddressByIdHttpTrigger>> _logger;

        [SetUp]
        public void Setup()
        {
            _address = new Models.Address();
            _request = new Mock<HttpRequest>();            
            _resourceHelper = new Mock<IResourceHelper>();            
            _httpRequestHelper = new Mock<IHttpRequestHelper>();
            _logger = new Mock<ILogger<AddressFunction.GetAddressByIdHttpTrigger>>();

            _getAddressByIdHttpTriggerService = new Mock<IGetAddressByIdHttpTriggerService>();
            _function = new AddressFunction.GetAddressByIdHttpTrigger(_resourceHelper.Object,
                _getAddressByIdHttpTriggerService.Object,                
                _httpRequestHelper.Object,
                _logger.Object);
        }

        [Test]
        public async Task GetAddressByIdHttpTrigger_ReturnsStatusCodeBadRequest_WhenTouchpointIdIsNotProvided()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request.Object)).Returns((string)null);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidAddressId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task GetAddressByIdHttpTrigger_ReturnsStatusCodeBadRequest_WhenCustomerIdIsInvalid()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request.Object)).Returns("0000000001");

            // Act
            var result = await RunFunction(InValidId, ValidAddressId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task GetAddressByIdHttpTrigger_ReturnsStatusCodeBadRequest_WhenAddressIdIsInvalid()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request.Object)).Returns("0000000001");

            // Act
            var result = await RunFunction(ValidCustomerId, InValidId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task GetAddressByIdHttpTrigger_ReturnsStatusCodeNoContent_WhenCustomerDoesNotExist()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request.Object)).Returns("0000000001");
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(false));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidAddressId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task GetAddressByIdHttpTrigger_ReturnsStatusCodeNoContent_WhenAddressDoesNotExist()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request.Object)).Returns("0000000001");
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _getAddressByIdHttpTriggerService.Setup(x => x.GetAddressForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult<Models.Address>(null));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidAddressId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task GetAddressByIdHttpTrigger_ReturnsStatusCodeOk_WhenAddressExists()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request.Object)).Returns("0000000001");
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _getAddressByIdHttpTriggerService.Setup(x => x.GetAddressForCustomerAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).Returns(Task.FromResult<Models.Address>(_address));

            // Act
            var result = await RunFunction(ValidCustomerId, ValidAddressId);

            // Assert
            Assert.That(result, Is.InstanceOf<JsonResult>());

            var responseResult = result as JsonResult;
            Assert.That(responseResult.StatusCode, Is.EqualTo((int)HttpStatusCode.OK));
        }

        private async Task<IActionResult> RunFunction(string customerId, string addressId)
        {
            return await _function.Run(
                _request.Object,                
                customerId,
                addressId).ConfigureAwait(false);
        }
    }
}
