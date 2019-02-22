using System;
using DFC.JSON.Standard;
using NCS.DSS.Address.Models;
using NCS.DSS.Address.PatchAddressHttpTrigger.Service;
using Newtonsoft.Json;
using NSubstitute;
using NUnit.Framework;

namespace NCS.DSS.Address.Tests.ServiceTests
{

    [TestFixture]
    public class AddressPatchServiceTests
    {
        private IJsonHelper _jsonHelper;
        private IAddressPatchService _addressPatchService;
        private AddressPatch _addressPatch;
        private string _json;

        [SetUp]
        public void Setup()
        {
            _jsonHelper = Substitute.For<JsonHelper>();
            _addressPatchService = Substitute.For<AddressPatchService>(_jsonHelper);
            _addressPatch = Substitute.For<AddressPatch>();

            _json = JsonConvert.SerializeObject(_addressPatch);
        }

        [Test]
        public void AddressPatchServiceTests_ReturnsNull_WhenAddressPatchIsNull()
        {
            var result = _addressPatchService.Patch(string.Empty, Arg.Any<AddressPatch>());

            // Assert
            Assert.IsNull(result);
        }

        [Test]
        public void AddressPatchServiceTests_CheckAddress1IsUpdated_WhenPatchIsCalled()
        {
            var addressPatch = new AddressPatch { Address1 = "Address 1" };

            var patchedAddress = _addressPatchService.Patch(_json, addressPatch);

            var address = JsonConvert.DeserializeObject<Models.Address>(patchedAddress);

            // Assert
            Assert.AreEqual("Address 1", address.Address1);
        }

        [Test]
        public void AddressPatchServiceTests_CheckAddress2IsUpdated_WhenPatchIsCalled()
        {
            var addressPatch = new AddressPatch { Address2 = "Address 2" };

            var patchedAddress = _addressPatchService.Patch(_json, addressPatch);

            var address = JsonConvert.DeserializeObject<Models.Address>(patchedAddress);

            // Assert
            Assert.AreEqual("Address 2", address.Address2);
        }

        [Test]
        public void AddressPatchServiceTests_CheckAddress3IsUpdated_WhenPatchIsCalled()
        {
            var addressPatch = new AddressPatch { Address3 = "Address 3" };

            var patchedAddress = _addressPatchService.Patch(_json, addressPatch);

            var address = JsonConvert.DeserializeObject<Models.Address>(patchedAddress);

            // Assert
            Assert.AreEqual("Address 3", address.Address3);
        }

        [Test]
        public void AddressPatchServiceTests_CheckAddress4IsUpdated_WhenPatchIsCalled()
        {
            var addressPatch = new AddressPatch { Address4 = "Address 4" };

            var patchedAddress = _addressPatchService.Patch(_json, addressPatch);

            var address = JsonConvert.DeserializeObject<Models.Address>(patchedAddress);

            // Assert
            Assert.AreEqual("Address 4", address.Address4);
        }

        [Test]
        public void AddressPatchServiceTests_CheckAddress5IsUpdated_WhenPatchIsCalled()
        {
            var addressPatch = new AddressPatch { Address5 = "Address 5" };

            var patchedAddress = _addressPatchService.Patch(_json, addressPatch);

            var address = JsonConvert.DeserializeObject<Models.Address>(patchedAddress);

            // Assert
            Assert.AreEqual("Address 5", address.Address5);
        }

        [Test]
        public void AddressPatchServiceTests_CheckPostCodeIsUpdated_WhenPatchIsCalled()
        {
            var addressPatch = new AddressPatch { PostCode = "CV1 1VC" };

            var patchedAddress = _addressPatchService.Patch(_json, addressPatch);

            var address = JsonConvert.DeserializeObject<Models.Address>(patchedAddress);

            // Assert
            Assert.AreEqual("CV1 1VC", address.PostCode);
        }

        [Test]
        public void AddressPatchServiceTests_CheckAlternativePostCodeIsUpdated_WhenPatchIsCalled()
        {
            var addressPatch = new AddressPatch { AlternativePostCode = "CV1 1VC" };

            var patchedAddress = _addressPatchService.Patch(_json, addressPatch);

            var address = JsonConvert.DeserializeObject<Models.Address>(patchedAddress);

            // Assert
            Assert.AreEqual("CV1 1VC", address.AlternativePostCode);
        }

        [Test]
        public void AddressPatchServiceTests_CheckLongitudeIsUpdated_WhenPatchIsCalled()
        {
            var addressPatch = new AddressPatch { Longitude = (decimal?)64.7511 };

            var patchedAddress = _addressPatchService.Patch(_json, addressPatch);

            var address = JsonConvert.DeserializeObject<Models.Address>(patchedAddress);

            // Assert
            Assert.AreEqual((decimal?)64.7511, address.Longitude);
        }

        [Test]
        public void AddressPatchServiceTests_CheckLatitudeIsUpdated_WhenPatchIsCalled()
        {
            var addressPatch = new AddressPatch { Latitude = (decimal?)147.3494 };

            var patchedAddress = _addressPatchService.Patch(_json, addressPatch);

            var address = JsonConvert.DeserializeObject<Models.Address>(patchedAddress);

            // Assert
            Assert.AreEqual((decimal?)147.3494, address.Latitude);
        }

        [Test]
        public void AddressPatchServiceTests_CheckEffectiveFromIsUpdated_WhenPatchIsCalled()
        {
            var addressPatch = new AddressPatch { EffectiveFrom = DateTime.MaxValue };

            var patchedAddress = _addressPatchService.Patch(_json, addressPatch);

            var address = JsonConvert.DeserializeObject<Models.Address>(patchedAddress);

            // Assert
            Assert.AreEqual(DateTime.MaxValue, address.EffectiveFrom);
        }

        [Test]
        public void AddressPatchServiceTests_CheckEffectiveToIsUpdated_WhenPatchIsCalled()
        {
            var addressPatch = new AddressPatch { EffectiveTo = DateTime.MaxValue };

            var patchedAddress = _addressPatchService.Patch(_json, addressPatch);

            var address = JsonConvert.DeserializeObject<Models.Address>(patchedAddress);

            // Assert
            Assert.AreEqual(DateTime.MaxValue, address.EffectiveTo);
        }

        [Test]
        public void AddressPatchServiceTests_CheckLastModifiedDateIsUpdated_WhenPatchIsCalled()
        {
            var addressPatch = new AddressPatch { LastModifiedDate = DateTime.MaxValue };

            var patchedAddress = _addressPatchService.Patch(_json, addressPatch);

            var address = JsonConvert.DeserializeObject<Models.Address>(patchedAddress);

            // Assert
            Assert.AreEqual(DateTime.MaxValue, address.LastModifiedDate);
        }

        [Test]
        public void AddressPatchServiceTests_CheckLastModifiedTouchpointIdIsUpdated_WhenPatchIsCalled()
        {
            var addressPatch = new AddressPatch { LastModifiedTouchpointId = "0000000111" };

            var patchedAddress = _addressPatchService.Patch(_json, addressPatch);

            var address = JsonConvert.DeserializeObject<Models.Address>(patchedAddress);

            // Assert
            Assert.AreEqual("0000000111", address.LastModifiedTouchpointId);
        }

        [Test]
        public void AddressPatchServiceTests_CheckSubcontractorIdIsUpdated_WhenPatchIsCalled()
        {
            var addressPatch = new AddressPatch { SubcontractorId = "0000000111" };

            var patchedAddress = _addressPatchService.Patch(_json, addressPatch);

            var address = JsonConvert.DeserializeObject<Models.Address>(patchedAddress);

            // Assert
            Assert.AreEqual("0000000111", address.SubcontractorId);
        }
    }
}
