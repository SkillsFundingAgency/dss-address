using System;
using DFC.JSON.Standard;
using Moq;
using NCS.DSS.Address.Models;
using NCS.DSS.Address.PatchAddressHttpTrigger.Service;
using Newtonsoft.Json;
using NUnit.Framework;
using Microsoft.Extensions.Logging;

namespace NCS.DSS.Address.Tests.ServiceTests
{

    [TestFixture]
    public class AddressPatchServiceTests
    {
        private IJsonHelper _jsonHelper;
        private IAddressPatchService _addressPatchService;
        private Models.Address _address;
        private string _json;
        private Mock<ILogger> _log;
        [SetUp]
        public void Setup()
        {
            _jsonHelper = new JsonHelper();
            _log = new Mock<ILogger>();
            _addressPatchService = new AddressPatchService(_jsonHelper);
            _address = new Models.Address() { 
                Address1 = "Some Address1", 
                Address2="some  Address2", 
                Address3="Some Address3", 
                Address4 = "some Address",
                Address5 = "SomeAddress5",
                AddressId = Guid.NewGuid(), 
                PostCode = "CV22 8BA",
                AlternativePostCode = "CV22 9BA"
            };

            _json = JsonConvert.SerializeObject(_address);
        }

        [Test]
        public void AddressPatchServiceTests_ReturnsNull_WhenAddressPatchIsNull()
        {
            var result = _addressPatchService.Patch(string.Empty, It.IsAny<AddressPatch>(),_log.Object);

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void AddressPatchServiceTests_CheckAddress1IsUpdated_WhenPatchIsCalled()
        {
            // Arrange
            var addressPatch = new AddressPatch { Address1 = "Address 1" };

            // Act
            var patchedAddress = _addressPatchService.Patch(_json, addressPatch,_log.Object);

            var address = JsonConvert.DeserializeObject<Models.Address>(patchedAddress);

            // Assert
            Assert.AreEqual("Address 1", address.Address1);
        }

        [Test]
        public void AddressPatchServiceTests_CheckAddress2IsUpdated_WhenPatchIsCalled()
        {
            // Arrange
            var addressPatch = new AddressPatch { Address2 = "Address 2" };

            // Act
            var patchedAddress = _addressPatchService.Patch(_json, addressPatch,_log.Object);

            var address = JsonConvert.DeserializeObject<Models.Address>(patchedAddress);

            // Assert
            Assert.AreEqual("Address 2", address.Address2);
        }

        [Test]
        public void AddressPatchServiceTests_CheckAddress3IsUpdated_WhenPatchIsCalled()
        {
            // Arrange
            var addressPatch = new AddressPatch { Address3 = "Address 3" };

            // Act
            var patchedAddress = _addressPatchService.Patch(_json, addressPatch,_log.Object);

            var address = JsonConvert.DeserializeObject<Models.Address>(patchedAddress);

            // Assert
            Assert.AreEqual("Address 3", address.Address3);
        }

        [Test]
        public void AddressPatchServiceTests_CheckAddress4IsUpdated_WhenPatchIsCalled()
        {
            // Arrange
            var addressPatch = new AddressPatch { Address4 = "Address 4" };

            // Act
            var patchedAddress = _addressPatchService.Patch(_json, addressPatch,_log.Object);

            var address = JsonConvert.DeserializeObject<Models.Address>(patchedAddress);

            // Assert
            Assert.AreEqual("Address 4", address.Address4);
        }

        [Test]
        public void AddressPatchServiceTests_CheckAddress5IsUpdated_WhenPatchIsCalled()
        {
            // Arrange
            var addressPatch = new AddressPatch { Address5 = "Address 5" };

            // Act
            var patchedAddress = _addressPatchService.Patch(_json, addressPatch,_log.Object);

            var address = JsonConvert.DeserializeObject<Models.Address>(patchedAddress);

            // Assert
            Assert.AreEqual("Address 5", address.Address5);
        }

        [Test]
        public void AddressPatchServiceTests_CheckPostCodeIsUpdated_WhenPatchIsCalled()
        {
            // Arrange
            var addressPatch = new AddressPatch { PostCode = "CV1 1VC" };

            // Act
            var patchedAddress = _addressPatchService.Patch(_json, addressPatch,_log.Object);

            var address = JsonConvert.DeserializeObject<Models.Address>(patchedAddress);

            // Assert
            Assert.AreEqual("CV1 1VC", address.PostCode);
        }

        [Test]
        public void AddressPatchServiceTests_CheckAlternativePostCodeIsUpdated_WhenPatchIsCalled()
        {
            // Arrange
            var addressPatch = new AddressPatch { AlternativePostCode = "CV1 1VC" };

            // Act
            var patchedAddress = _addressPatchService.Patch(_json, addressPatch,_log.Object);

            var address = JsonConvert.DeserializeObject<Models.Address>(patchedAddress);

            // Assert
            Assert.AreEqual("CV1 1VC", address.AlternativePostCode);
        }

        [Test]
        public void AddressPatchServiceTests_CheckLongitudeIsUpdated_WhenPatchIsCalled()
        {
            // Arrange
            var addressPatch = new AddressPatch { Longitude = (decimal?)64.7511 };

            // Act
            var patchedAddress = _addressPatchService.Patch(_json, addressPatch,_log.Object);

            var address = JsonConvert.DeserializeObject<Models.Address>(patchedAddress);

            // Assert
            Assert.AreEqual((decimal?)64.7511, address.Longitude);
        }

        [Test]
        public void AddressPatchServiceTests_CheckLatitudeIsUpdated_WhenPatchIsCalled()
        {
            // Arrange
            var addressPatch = new AddressPatch { Latitude = (decimal?)147.3494 };

            // Act
            var patchedAddress = _addressPatchService.Patch(_json, addressPatch,_log.Object);

            var address = JsonConvert.DeserializeObject<Models.Address>(patchedAddress);

            // Assert
            Assert.AreEqual((decimal?)147.3494, address.Latitude);
        }

        [Test]
        public void AddressPatchServiceTests_CheckEffectiveFromIsUpdated_WhenPatchIsCalled()
        {
            // Arrange
            var addressPatch = new AddressPatch { EffectiveFrom = DateTime.MaxValue };

            // Act
            var patchedAddress = _addressPatchService.Patch(_json, addressPatch,_log.Object);

            var address = JsonConvert.DeserializeObject<Models.Address>(patchedAddress);

            // Assert
            Assert.AreEqual(DateTime.MaxValue, address.EffectiveFrom);
        }

        [Test]
        public void AddressPatchServiceTests_CheckEffectiveToIsUpdated_WhenPatchIsCalled()
        {
            // Arrange
            var addressPatch = new AddressPatch { EffectiveTo = DateTime.MaxValue };

            // Act
            var patchedAddress = _addressPatchService.Patch(_json, addressPatch,_log.Object);

            var address = JsonConvert.DeserializeObject<Models.Address>(patchedAddress);

            // Assert
            Assert.AreEqual(DateTime.MaxValue, address.EffectiveTo);
        }

        [Test]
        public void AddressPatchServiceTests_CheckLastModifiedDateIsUpdated_WhenPatchIsCalled()
        {
            // Arrange
            var addressPatch = new AddressPatch { LastModifiedDate = DateTime.MaxValue };

            // Act
            var patchedAddress = _addressPatchService.Patch(_json, addressPatch,_log.Object);

            var address = JsonConvert.DeserializeObject<Models.Address>(patchedAddress);

            // Assert
            Assert.AreEqual(DateTime.MaxValue, address.LastModifiedDate);
        }

        [Test]
        public void AddressPatchServiceTests_CheckLastModifiedTouchpointIdIsUpdated_WhenPatchIsCalled()
        {
            // Arrange
            var addressPatch = new AddressPatch { LastModifiedTouchpointId = "0000000111" };

            // Act
            var patchedAddress = _addressPatchService.Patch(_json, addressPatch,_log.Object);

            var address = JsonConvert.DeserializeObject<Models.Address>(patchedAddress);

            // Assert
            Assert.AreEqual("0000000111", address.LastModifiedTouchpointId);
        }

        [Test]
        public void AddressPatchServiceTests_CheckSubcontractorIdIsUpdated_WhenPatchIsCalled()
        {
            // Arrange
            var addressPatch = new AddressPatch { SubcontractorId = "0000000111" };

            // Act
            var patchedAddress = _addressPatchService.Patch(_json, addressPatch,_log.Object);

            var address = JsonConvert.DeserializeObject<Models.Address>(patchedAddress);

            // Assert
            Assert.AreEqual("0000000111", address.SubcontractorId);
        }

        [Test]
        public void AddressPatchServiceTests_PatchesAddress2EmptyString_WhenPatchIsCalledWithEmptyString()
        {
            // Arrange
            var addressPatch = new AddressPatch { Address2 = "" };

            // Act
            var patchedAddress = _addressPatchService.Patch(_json, addressPatch,_log.Object);
            var address = JsonConvert.DeserializeObject<Models.Address>(patchedAddress);

            // Assert
            Assert.IsEmpty(address.Address2);
        }

        [Test]
        public void AddressPatchServiceTests_PatchesAddress3EmptyString_WhenPatchIsCalledWithEmptyString()
        {
            // Arrange
            var addressPatch = new AddressPatch { Address3 = "" };

            // Act
            var patchedAddress = _addressPatchService.Patch(_json, addressPatch,_log.Object);
            var address = JsonConvert.DeserializeObject<Models.Address>(patchedAddress);

            // Assert
            Assert.IsEmpty(address.Address3);
        }


        [Test]
        public void AddressPatchServiceTests_PatchesAddress4EmptyString_WhenPatchIsCalledWithEmptyString()
        {
            // Arrange
            var addressPatch = new AddressPatch { Address4 = "" };

            // Act
            var patchedAddress = _addressPatchService.Patch(_json, addressPatch,_log.Object);
            var address = JsonConvert.DeserializeObject<Models.Address>(patchedAddress);

            // Assert
            Assert.IsEmpty(address.Address4);
        }

        [Test]
        public void AddressPatchServiceTests_PatchesAddress5EmptyString_WhenPatchIsCalledWithEmptyString()
        {
            // Arrange
            var addressPatch = new AddressPatch { Address5 = "" };

            // Act
            var patchedAddress = _addressPatchService.Patch(_json, addressPatch,_log.Object);
            var address = JsonConvert.DeserializeObject<Models.Address>(patchedAddress);

            // Assert
            Assert.IsEmpty(address.Address5);
        }

        [Test]
        public void AddressPatchServiceTests_PatchesAlternativePostcodeEmptyString_WhenPatchIsCalledWithEmptyString()
        {
            // Arrange
            var addressPatch = new AddressPatch { AlternativePostCode = "" };

            // Act
            var patchedAddress = _addressPatchService.Patch(_json, addressPatch,_log.Object);
            var address = JsonConvert.DeserializeObject<Models.Address>(patchedAddress);

            // Assert
            Assert.IsEmpty(address.AlternativePostCode);
        }
    }
}
