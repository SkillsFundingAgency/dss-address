using NCS.DSS.Address.Models;
using System.ComponentModel.DataAnnotations;

namespace NCS.DSS.Address.Validation
{
    public interface IValidate
    {
        List<ValidationResult> ValidateResource(IAddress resource, bool validateModelForPost);
    }
}