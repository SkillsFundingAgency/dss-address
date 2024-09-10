using Moq;
using NUnit.Framework;
using System;

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

            Assert.That(address.AddressId, Is.Not.Null);
            Assert.That(address.LastModifiedDate, Is.Not.Null);
        }

        [Test]
        public void AddressTests_CheckLastModifiedDateDoesNotGetPopulated_WhenSetDefaultValuesIsCalled()
        {
            var address = new Models.Address { LastModifiedDate = DateTime.MaxValue };

            address.SetDefaultValues();

            // Assert            
            Assert.That(address.LastModifiedDate, Is.EqualTo(DateTime.MaxValue));
        }

        [Test]
        public void AddressTests_CheckAddressIdIsSet_WhenSetIdsIsCalled()
        {
            var address = new Models.Address();

            address.SetIds(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>());

            // Assert            
            Assert.That(address.AddressId, Is.Not.SameAs(Guid.Empty));
        }

        [Test]
        public void AddressTests_CheckCustomerIdIsSet_WhenSetIdsIsCalled()
        {
            var address = new Models.Address();

            var customerId = Guid.NewGuid();
            address.SetIds(customerId, It.IsAny<string>(), It.IsAny<string>());

            // Assert            
            Assert.That(address.CustomerId, Is.EqualTo(customerId));
        }

        [Test]
        public void AddressTests_CheckLastModifiedTouchpointIdIsSet_WhenSetIdsIsCalled()
        {
            var address = new Models.Address();

            address.SetIds(It.IsAny<Guid>(), "0000000000", It.IsAny<string>());

            // Assert            
            Assert.That(address.LastModifiedTouchpointId, Is.EqualTo("0000000000"));
        }
    }
}
