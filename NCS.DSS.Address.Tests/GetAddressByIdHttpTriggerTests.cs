﻿using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions;
using Microsoft.Azure.WebJobs.Host;
using NCS.DSS.Address.Cosmos.Helper;
using NCS.DSS.Address.GetAddressByIdHttpTrigger.Service;
using NSubstitute;
using NUnit.Framework;

namespace NCS.DSS.Address.Tests
{
    [TestFixture]
    public class GetAddressByIdHttpTriggerTests
    {
        private const string ValidCustomerId = "7E467BDB-213F-407A-B86A-1954053D3C24";
        private const string ValidAddressId = "1e1a555c-9633-4e12-ab28-09ed60d51cb3";
        private const string InValidId = "1111111-2222-3333-4444-555555555555";
        private readonly Guid _addressId = Guid.Parse("aa57e39e-4469-4c79-a9e9-9cb4ef410382");
        private TraceWriter _log;
        private HttpRequestMessage _request;
        private IResourceHelper _resourceHelper;
        private IGetAddressByIdHttpTriggerService _getAddressByIdHttpTriggerService;
        private Models.Address _address;

        [SetUp]
        public void Setup()
        {
            _address = Substitute.For<Models.Address>();

            _request = new HttpRequestMessage()
            {
                Content = new StringContent(string.Empty),
                RequestUri = 
                    new Uri($"http://localhost:7071/api/Customers/7E467BDB-213F-407A-B86A-1954053D3C24/Addressess/1e1a555c-9633-4e12-ab28-09ed60d51cb")
            };
            _log = new TraceMonitor();
            _resourceHelper = Substitute.For<IResourceHelper>();
            _getAddressByIdHttpTriggerService = Substitute.For<IGetAddressByIdHttpTriggerService>();
        }

        [Test]
        public async Task GetAddressHttpTrigger_ReturnsStatusCodeBadRequest_WhenCustomerIdIsInvalid()
        {
            // Act
            var result = await RunFunction(InValidId, ValidAddressId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task GetAddressHttpTrigger_ReturnsStatusCodeBadRequest_WhenAddressIdIsInvalid()
        {
            // Act
            var result = await RunFunction(ValidCustomerId, InValidId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task GetAddressHttpTrigger_ReturnsStatusCodeNoContent_WhenCustomerDoesNotExist()
        {
            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).ReturnsForAnyArgs(false);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidAddressId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Test]
        public async Task GetAddressHttpTrigger_ReturnsStatusCodeNoContent_WhenAddressDoesNotExist()
        {
            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(true);

            _getAddressByIdHttpTriggerService.GetAddressForCustomerAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(Task.FromResult<Models.Address>(null).Result);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidAddressId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Test]
        public async Task GetAddressHttpTrigger_ReturnsStatusCodeOk_WhenAddressExists()
        {
            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(true);

            _getAddressByIdHttpTriggerService.GetAddressForCustomerAsync(Arg.Any<Guid>(), Arg.Any<Guid>()).Returns(Task.FromResult<Models.Address>(_address).Result);

            // Act
            var result = await RunFunction(ValidCustomerId, ValidAddressId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
        }

        private async Task<HttpResponseMessage> RunFunction(string customerId, string addressId)
        {
            return await GetAddressByIdHttpTrigger.Function.GetAddressByIdHttpTrigger.Run(
                _request, _log, customerId, addressId, _resourceHelper, _getAddressByIdHttpTriggerService).ConfigureAwait(false);
        }

    }
}