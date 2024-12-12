using DFC.Common.Standard.Logging;
using DFC.HTTP.Standard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NCS.DSS.Address.Cosmos.Helper;
using NCS.DSS.Address.GetAddressHttpTrigger.Service;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using AddressFunction = NCS.DSS.Address.GetAddressHttpTrigger.Function;

namespace NCS.DSS.Address.Tests.FunctionTest
{
    [TestFixture]
    public class GetAddressHttpTriggerTests
    {
        private const string ValidCustomerId = "7E467BDB-213F-407A-B86A-1954053D3C24";
        private const string InValidId = "1111111-2222-3333-4444-555555555555";
        private HttpRequest _request;
        private Mock<ILogger> _log;
        private Mock<IResourceHelper> _resourceHelper;        
        private Mock<IHttpRequestHelper> _httpRequestHelper;
        private Mock<IGetAddressHttpTriggerService> _getAddressHttpTriggerService;
        private AddressFunction.GetAddressHttpTrigger _function;
        private Mock<ILogger<AddressFunction.GetAddressHttpTrigger>> _logger;

        [SetUp]
        public void Setup()
        {
            _request = new DefaultHttpContext().Request;
            _resourceHelper = new Mock<IResourceHelper>();            
            _httpRequestHelper = new Mock<IHttpRequestHelper>();
            _logger = new Mock<ILogger<AddressFunction.GetAddressHttpTrigger>>();
            _getAddressHttpTriggerService = new Mock<IGetAddressHttpTriggerService>();
            _function = new AddressFunction.GetAddressHttpTrigger(_resourceHelper.Object, _getAddressHttpTriggerService.Object, _httpRequestHelper.Object, _logger.Object);
        }

        [Test]
        public async Task GetAddressHttpTrigger_ReturnsStatusCodeBadRequest_WhenTouchpointIdIsNotProvided()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns((string)null);

            // Act
            var result = await RunFunction(ValidCustomerId);

            // Assert            
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task GetAddressHttpTrigger_ReturnsStatusCodeBadRequest_WhenCustomerIdIsInvalid()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");

            // Act
            var result = await RunFunction(InValidId);

            // Assert
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task GetAddressHttpTrigger_ReturnsStatusCodeNoContent_WhenCustomerDoesNotExist()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(false));

            // Act
            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task GetAddressHttpTrigger_ReturnsStatusCodeNoContent_WhenAddressDoesNotExist()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            _getAddressHttpTriggerService.Setup(x => x.GetAddressesAsync(It.IsAny<Guid>())).Returns(Task.FromResult<List<Models.Address>>(null));

            // Act
            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task GetAddressHttpTrigger_ReturnsStatusCodeOk_WhenAddressExists()
        {
            // Arrange
            _httpRequestHelper.Setup(x => x.GetDssTouchpointId(_request)).Returns("0000000001");
            _resourceHelper.Setup(x => x.DoesCustomerExist(It.IsAny<Guid>())).Returns(Task.FromResult(true));
            var listOfAddresses = new List<Models.Address>();
            _getAddressHttpTriggerService.Setup(x => x.GetAddressesAsync(It.IsAny<Guid>())).Returns(Task.FromResult<List<Models.Address>>(listOfAddresses));

            // Act
            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.That(result, Is.InstanceOf<JsonResult>());

            var responseResult = result as JsonResult;
            Assert.That(responseResult.StatusCode, Is.EqualTo((int)HttpStatusCode.OK));
        }

        private async Task<IActionResult> RunFunction(string customerId)
        {
            return await _function.Run(
                _request,
                customerId).ConfigureAwait(false);
        }
    }
}