using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions;
using Microsoft.Azure.WebJobs.Host;
using NCS.DSS.Address.Cosmos.Helper;
using NCS.DSS.Address.GetAddressHttpTrigger.Service;
using NSubstitute;
using NUnit.Framework;

namespace NCS.DSS.Address.Tests
{
    [TestFixture]
    public class GetAddressHttpTriggerTests
    {
        private const string ValidCustomerId = "7E467BDB-213F-407A-B86A-1954053D3C24";
        private const string InValidId = "1111111-2222-3333-4444-555555555555";
        private TraceWriter _log;
        private HttpRequestMessage _request;
        private IResourceHelper _resourceHelper;
        private IGetAddressHttpTriggerService _getAddressHttpTriggerService;

        [SetUp]
        public void Setup()
        {
            _request = new HttpRequestMessage()
            {
                Content = new StringContent(string.Empty),
                RequestUri = 
                    new Uri($"http://localhost:7071/api/Customers/7E467BDB-213F-407A-B86A-1954053D3C24/Addressess/")
            };
            _log = new TraceMonitor();
            _resourceHelper = Substitute.For<IResourceHelper>();
            _getAddressHttpTriggerService = Substitute.For<IGetAddressHttpTriggerService>();
        }

        [Test]
        public async Task GetAddressHttpTrigger_ReturnsStatusCodeBadRequest_WhenCustomerIdIsInvalid()
        {
            // Act
            var result = await RunFunction(InValidId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Test]
        public async Task GetAddressHttpTrigger_ReturnsStatusCodeNoContent_WhenCustomerDoesntExist()
        {
            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).ReturnsForAnyArgs(false);

            // Act
            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Test]
        public async Task GetAddressHttpTrigger_ReturnsStatusCodeNoContent_WhenAddressDoesntExist()
        {
            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(true);

            _getAddressHttpTriggerService.GetAddressesAsync(Arg.Any<Guid>()).Returns(Task.FromResult<List<Models.Address>>(null).Result);

            // Act
            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Test]
        public async Task GetAddressHttpTrigger_ReturnsStatusCodeOk_WhenAddressExists()
        {
            _resourceHelper.DoesCustomerExist(Arg.Any<Guid>()).Returns(true);

            var listOfAddresses = new List<Models.Address>();
            _getAddressHttpTriggerService.GetAddressesAsync(Arg.Any<Guid>()).Returns(Task.FromResult<List<Models.Address>>(listOfAddresses).Result);

            // Act
            var result = await RunFunction(ValidCustomerId);

            // Assert
            Assert.IsInstanceOf<HttpResponseMessage>(result);
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
        }

        private async Task<HttpResponseMessage> RunFunction(string customerId)
        {
            return await GetAddressHttpTrigger.Function.GetAddressHttpTrigger.Run(
                _request, _log, customerId, _resourceHelper, _getAddressHttpTriggerService).ConfigureAwait(false);
        }

    }
}
