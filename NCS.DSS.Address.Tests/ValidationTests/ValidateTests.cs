using NCS.DSS.Address.Validation;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NCS.DSS.Address.Tests.ValidationTests
{
    [TestFixture]
    public class ValidateTests
    {

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenAddressIsNotSuppliedForPost()
        {
            // Arrange
            var address = new Models.Address
            {
                PostCode = "CV1 1VC"
            };

            var validation = new Validate();

            // Act
            var result = validation.ValidateResource(address, true);

            // Assert
            Assert.That(result, Is.InstanceOf<List<ValidationResult>>());
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(2));
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenPostcodeIsNotSuppliedForPost()
        {
            // Arrange
            var address = new Models.Address
            {
                Address1 = "Address Line 1"
            };

            var validation = new Validate();

            // Act
            var result = validation.ValidateResource(address, true);

            // Assert
            Assert.That(result, Is.InstanceOf<List<ValidationResult>>());
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(2));
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenEffectiveFromIsInTheFuture()
        {
            // Arrange
            var address = new Models.Address
            {
                Address1 = "Address Line 1",
                PostCode = "CV1 1VC",
                EffectiveFrom = DateTime.MaxValue
            };

            var validation = new Validate();

            // Act
            var result = validation.ValidateResource(address, false);

            // Assert
            Assert.That(result, Is.InstanceOf<List<ValidationResult>>());
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenLastModifiedDateIsInTheFuture()
        {
            // Arrange
            var address = new Models.Address
            {
                Address1 = "Address Line 1",
                PostCode = "CV1 1VC",
                LastModifiedDate = DateTime.MaxValue
            };

            var validation = new Validate();

            // Act
            var result = validation.ValidateResource(address, false);

            // Assert
            Assert.That(result, Is.InstanceOf<List<ValidationResult>>());
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));
        }

    }
}
