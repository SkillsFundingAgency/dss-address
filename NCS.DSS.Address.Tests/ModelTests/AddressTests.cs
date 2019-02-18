using System;
using NSubstitute;
using NUnit.Framework;

namespace NCS.DSS.Address.Tests.ModelTests
{
    [TestFixture]
    public class AddressTests
    {

        [Test]
        public void AddressTests_PopulatesDefaultValues_WhenSetDefaultValuesIsCalled()
        {
            var address = new Models.Address();
            address.SetDefaultValues();

            // Assert
            Assert.IsNotNull(address.AddressId);
            Assert.IsNotNull(address.LastModifiedDate);
        }

        [Test]
        public void AddressTests_CheckLastModifiedDateDoesNotGetPopulated_WhenSetDefaultValuesIsCalled()
        {
            var address = new Models.Address { LastModifiedDate = DateTime.MaxValue };

            address.SetDefaultValues();

            // Assert
            Assert.AreEqual(DateTime.MaxValue, address.LastModifiedDate);
        }

        [Test]
        public void AddressTests_CheckAddressIdIsSet_WhenSetIdsIsCalled()
        {
            var address = new Models.Address();

            address.SetIds(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>());

            // Assert
            Assert.AreNotSame(Guid.Empty, address.AddressId);
        }

        [Test]
        public void AddressTests_CheckCustomerIdIsSet_WhenSetIdsIsCalled()
        {
            var address = new Models.Address();

            var customerId = Guid.NewGuid();
            address.SetIds(customerId, Arg.Any<string>(), Arg.Any<string>());

            // Assert
            Assert.AreEqual(customerId, address.CustomerId);
        }

       [Test]
        public void AddressTests_CheckLastModifiedTouchpointIdIsSet_WhenSetIdsIsCalled()
        {
            var address = new Models.Address();

            address.SetIds(Arg.Any<Guid>(), "0000000000", Arg.Any<string>());

            // Assert
            Assert.AreEqual("0000000000", address.LastModifiedTouchpointId);
        }
    }
}
