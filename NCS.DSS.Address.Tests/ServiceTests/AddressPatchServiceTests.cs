using DFC.JSON.Standard;
using Microsoft.Extensions.Logging;
using Moq;
using NCS.DSS.Address.Models;
using NCS.DSS.Address.PatchAddressHttpTrigger.Service;
using Newtonsoft.Json;
using NUnit.Framework;
using System;

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
            _address = new Models.Address()
            {
                Address1 = "Some Address1",
                Address2 = "some  Address2",
                Address3 = "Some Address3",
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
            var result = _addressPatchService.Patch(string.Empty, It.IsAny<AddressPatch>(), _log.Object);

            // Assert            
            Assert.That(result, Is.Null);
        }

        [Test]
        public void AddressPatchServiceTests_CheckAddress1IsUpdated_WhenPatchIsCalled()
        {
            // Arrange
            var addressPatch = new AddressPatch { Address1 = "Address 1" };

            // Act
            var patchedAddress = _addressPatchService.Patch(_json, addressPatch, _log.Object);

            var address = JsonConvert.DeserializeObject<Models.Address>(patchedAddress);

            // Assert            
            Assert.That(address.Address1, Is.EqualTo("Address 1"));
        }

        [Test]
        public void AddressPatchServiceTests_CheckAddress2IsUpdated_WhenPatchIsCalled()
        {
            // Arrange
            var addressPatch = new AddressPatch { Address2 = "Address 2" };

            // Act
            var patchedAddress = _addressPatchService.Patch(_json, addressPatch, _log.Object);

            var address = JsonConvert.DeserializeObject<Models.Address>(patchedAddress);

            // Assert            
            Assert.That(address.Address2, Is.EqualTo("Address 2"));
        }

        [Test]
        public void AddressPatchServiceTests_CheckAddress3IsUpdated_WhenPatchIsCalled()
        {
            // Arrange
            var addressPatch = new AddressPatch { Address3 = "Address 3" };

            // Act
            var patchedAddress = _addressPatchService.Patch(_json, addressPatch, _log.Object);

            var address = JsonConvert.DeserializeObject<Models.Address>(patchedAddress);

            // Assert            
            Assert.That(address.Address3, Is.EqualTo("Address 3"));
        }

        [Test]
        public void AddressPatchServiceTests_CheckAddress4IsUpdated_WhenPatchIsCalled()
        {
            // Arrange
            var addressPatch = new AddressPatch { Address4 = "Address 4" };

            // Act
            var patchedAddress = _addressPatchService.Patch(_json, addressPatch, _log.Object);

            var address = JsonConvert.DeserializeObject<Models.Address>(patchedAddress);

            // Assert            
            Assert.That(address.Address4, Is.EqualTo("Address 4"));
        }

        [Test]
        public void AddressPatchServiceTests_CheckAddress5IsUpdated_WhenPatchIsCalled()
        {
            // Arrange
            var addressPatch = new AddressPatch { Address5 = "Address 5" };

            // Act
            var patchedAddress = _addressPatchService.Patch(_json, addressPatch, _log.Object);

            var address = JsonConvert.DeserializeObject<Models.Address>(patchedAddress);

            // Assert            
            Assert.That(address.Address5, Is.EqualTo("Address 5"));
        }

        [Test]
        public void AddressPatchServiceTests_CheckPostCodeIsUpdated_WhenPatchIsCalled()
        {
            // Arrange
            var addressPatch = new AddressPatch { PostCode = "CV1 1VC" };

            // Act
            var patchedAddress = _addressPatchService.Patch(_json, addressPatch, _log.Object);

            var address = JsonConvert.DeserializeObject<Models.Address>(patchedAddress);

            // Assert            
            Assert.That(address.PostCode, Is.EqualTo("CV1 1VC"));
        }

        [Test]
        public void AddressPatchServiceTests_CheckAlternativePostCodeIsUpdated_WhenPatchIsCalled()
        {
            // Arrange
            var addressPatch = new AddressPatch { AlternativePostCode = "CV1 1VC" };

            // Act
            var patchedAddress = _addressPatchService.Patch(_json, addressPatch, _log.Object);

            var address = JsonConvert.DeserializeObject<Models.Address>(patchedAddress);

            // Assert            
            Assert.That(address.AlternativePostCode, Is.EqualTo("CV1 1VC"));
        }

        [Test]
        public void AddressPatchServiceTests_CheckLongitudeIsUpdated_WhenPatchIsCalled()
        {
            // Arrange
            var addressPatch = new AddressPatch { Longitude = (decimal?)64.7511 };

            // Act
            var patchedAddress = _addressPatchService.Patch(_json, addressPatch, _log.Object);

            var address = JsonConvert.DeserializeObject<Models.Address>(patchedAddress);

            // Assert            
            Assert.That(address.Longitude, Is.EqualTo((decimal?)64.7511));
        }

        [Test]
        public void AddressPatchServiceTests_CheckLatitudeIsUpdated_WhenPatchIsCalled()
        {
            // Arrange
            var addressPatch = new AddressPatch { Latitude = (decimal?)147.3494 };

            // Act
            var patchedAddress = _addressPatchService.Patch(_json, addressPatch, _log.Object);

            var address = JsonConvert.DeserializeObject<Models.Address>(patchedAddress);

            // Assert            
            Assert.That(address.Latitude, Is.EqualTo((decimal?)147.3494));
        }

        [Test]
        public void AddressPatchServiceTests_CheckEffectiveFromIsUpdated_WhenPatchIsCalled()
        {
            // Arrange
            var addressPatch = new AddressPatch { EffectiveFrom = DateTime.MaxValue };

            // Act
            var patchedAddress = _addressPatchService.Patch(_json, addressPatch, _log.Object);

            var address = JsonConvert.DeserializeObject<Models.Address>(patchedAddress);

            // Assert            
            Assert.That(address.EffectiveFrom, Is.EqualTo(DateTime.MaxValue));
        }

        [Test]
        public void AddressPatchServiceTests_CheckEffectiveToIsUpdated_WhenPatchIsCalled()
        {
            // Arrange
            var addressPatch = new AddressPatch { EffectiveTo = DateTime.MaxValue };

            // Act
            var patchedAddress = _addressPatchService.Patch(_json, addressPatch, _log.Object);

            var address = JsonConvert.DeserializeObject<Models.Address>(patchedAddress);

            // Assert            
            Assert.That(address.EffectiveTo, Is.EqualTo(DateTime.MaxValue));
        }

        [Test]
        public void AddressPatchServiceTests_CheckLastModifiedDateIsUpdated_WhenPatchIsCalled()
        {
            // Arrange
            var addressPatch = new AddressPatch { LastModifiedDate = DateTime.MaxValue };

            // Act
            var patchedAddress = _addressPatchService.Patch(_json, addressPatch, _log.Object);

            var address = JsonConvert.DeserializeObject<Models.Address>(patchedAddress);

            // Assert            
            Assert.That(address.LastModifiedDate, Is.EqualTo(DateTime.MaxValue));
        }

        [Test]
        public void AddressPatchServiceTests_CheckLastModifiedTouchpointIdIsUpdated_WhenPatchIsCalled()
        {
            // Arrange
            var addressPatch = new AddressPatch { LastModifiedTouchpointId = "0000000111" };

            // Act
            var patchedAddress = _addressPatchService.Patch(_json, addressPatch, _log.Object);

            var address = JsonConvert.DeserializeObject<Models.Address>(patchedAddress);

            // Assert            
            Assert.That(address.LastModifiedTouchpointId, Is.EqualTo("0000000111"));
        }

        [Test]
        public void AddressPatchServiceTests_CheckSubcontractorIdIsUpdated_WhenPatchIsCalled()
        {
            // Arrange
            var addressPatch = new AddressPatch { SubcontractorId = "0000000111" };

            // Act
            var patchedAddress = _addressPatchService.Patch(_json, addressPatch, _log.Object);

            var address = JsonConvert.DeserializeObject<Models.Address>(patchedAddress);

            // Assert            
            Assert.That(address.SubcontractorId, Is.EqualTo("0000000111"));
        }

        [Test]
        public void AddressPatchServiceTests_PatchesAddress2EmptyString_WhenPatchIsCalledWithEmptyString()
        {
            // Arrange
            var addressPatch = new AddressPatch { Address2 = "" };

            // Act
            var patchedAddress = _addressPatchService.Patch(_json, addressPatch, _log.Object);
            var address = JsonConvert.DeserializeObject<Models.Address>(patchedAddress);

            // Assert            
            Assert.That(address.Address2, Is.Empty);
        }

        [Test]
        public void AddressPatchServiceTests_PatchesAddress3EmptyString_WhenPatchIsCalledWithEmptyString()
        {
            // Arrange
            var addressPatch = new AddressPatch { Address3 = "" };

            // Act
            var patchedAddress = _addressPatchService.Patch(_json, addressPatch, _log.Object);
            var address = JsonConvert.DeserializeObject<Models.Address>(patchedAddress);

            // Assert            
            Assert.That(address.Address3, Is.Empty);
        }


        [Test]
        public void AddressPatchServiceTests_PatchesAddress4EmptyString_WhenPatchIsCalledWithEmptyString()
        {
            // Arrange
            var addressPatch = new AddressPatch { Address4 = "" };

            // Act
            var patchedAddress = _addressPatchService.Patch(_json, addressPatch, _log.Object);
            var address = JsonConvert.DeserializeObject<Models.Address>(patchedAddress);

            // Assert            
            Assert.That(address.Address4, Is.Empty);
        }

        [Test]
        public void AddressPatchServiceTests_PatchesAddress5EmptyString_WhenPatchIsCalledWithEmptyString()
        {
            // Arrange
            var addressPatch = new AddressPatch { Address5 = "" };

            // Act
            var patchedAddress = _addressPatchService.Patch(_json, addressPatch, _log.Object);
            var address = JsonConvert.DeserializeObject<Models.Address>(patchedAddress);

            // Assert            
            Assert.That(address.Address5, Is.Empty);
        }

        [Test]
        public void AddressPatchServiceTests_PatchesAlternativePostcodeEmptyString_WhenPatchIsCalledWithEmptyString()
        {
            // Arrange
            var addressPatch = new AddressPatch { AlternativePostCode = "" };

            // Act
            var patchedAddress = _addressPatchService.Patch(_json, addressPatch, _log.Object);
            var address = JsonConvert.DeserializeObject<Models.Address>(patchedAddress);

            // Assert            
            Assert.That(address.AlternativePostCode, Is.Empty);
        }
    }
}
