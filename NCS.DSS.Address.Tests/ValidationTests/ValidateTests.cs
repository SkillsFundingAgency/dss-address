using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NCS.DSS.Address.Validation;
using NUnit.Framework;

namespace NCS.DSS.Address.Tests.ValidationTests
{
    [TestFixture]
    public class ValidateTests
    {
        
        [Test]
        public void ValidateTests_ReturnValidationResult_WhenAddressIsNotSuppliedForPost()
        {
            var address = new Models.Address
            {
               PostCode = "CV1 1VC"
            };

            var validation = new Validate();

            var result = validation.ValidateResource(address, true);

            // Assert
            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenPostcodeIsNotSuppliedForPost()
        {
            var address = new Models.Address
            {
                Address1 = "Address Line 1"
            };

            var validation = new Validate();

            var result = validation.ValidateResource(address, true);

            // Assert
            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count);
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenEffectiveFromIsInTheFuture()
        {
            var address = new Models.Address
            {
                Address1 = "Address Line 1",
                PostCode = "CV1 1VC",
                EffectiveFrom = DateTime.MaxValue
            };

            var validation = new Validate();

            var result = validation.ValidateResource(address, false);

            // Assert
            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
        }

        [Test]
        public void ValidateTests_ReturnValidationResult_WhenLastModifiedDateIsInTheFuture()
        {
            var address = new Models.Address
            {
                Address1 = "Address Line 1",
                PostCode = "CV1 1VC",
                LastModifiedDate = DateTime.MaxValue
            };

            var validation = new Validate();

            var result = validation.ValidateResource(address, false);

            // Assert
            Assert.IsInstanceOf<List<ValidationResult>>(result);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Count);
        }

    }
}
