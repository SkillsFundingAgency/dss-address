using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using NCS.DSS.Address.Models;

namespace NCS.DSS.Address.Validation
{
    public interface IValidate
    {
        List<ValidationResult> ValidateResource(IAddress resource);
    }
}