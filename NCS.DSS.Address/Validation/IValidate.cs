using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NCS.DSS.Address.Validation
{
    public interface IValidate
    {
        List<ValidationResult> ValidateResource<T>(T resource);
    }
}