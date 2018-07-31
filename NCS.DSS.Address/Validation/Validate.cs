using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NCS.DSS.Address.Models;

namespace NCS.DSS.Address.Validation
{
    public class Validate : IValidate
    {
        public List<ValidationResult> ValidateResource(IAddress resource, bool validateModelForPost)
        {
            var context = new ValidationContext(resource, null, null);
            var results = new List<ValidationResult>();

            Validator.TryValidateObject(resource, context, results, true);
            ValidateAddressRules(resource, results, validateModelForPost);
            return results;
        }

        private void ValidateAddressRules(IAddress addressResource, List<ValidationResult> results, bool validateModelForPost)
        {
            if (addressResource == null)
                return;

            if (validateModelForPost)
            {
                if (string.IsNullOrWhiteSpace(addressResource.Address1))
                    results.Add(new ValidationResult("Address 1 is a required field", new[] { "Address1" }));

                if (string.IsNullOrWhiteSpace(addressResource.PostCode))
                    results.Add(new ValidationResult("PostCode is a required field", new[] { "PostCode" }));
            }

            if (addressResource.EffectiveFrom.HasValue && addressResource.EffectiveFrom.Value > DateTime.UtcNow)
                results.Add(new ValidationResult("Effective From Agreed must be less the current date/time", new[] { "EffectiveFrom" }));

            if (addressResource.LastModifiedDate.HasValue && addressResource.LastModifiedDate.Value > DateTime.UtcNow)
                results.Add(new ValidationResult("Last Modified Date must be less the current date/time", new[] { "LastModifiedDate" }));
        }

    }
}
