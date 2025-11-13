using FluentValidation.Results;
using Orcamentaria.Lib.Domain.Validators;
using Orcamentaria.MaterialService.Domain.Models;

namespace Orcamentaria.MaterialService.Domain.Validators
{
    public interface IMaterialValidator : IValidatorEntity<Material>
    {
        ValidationResult ValidateSuppliersAsync(IEnumerable<long> suppliers);
    }
}
