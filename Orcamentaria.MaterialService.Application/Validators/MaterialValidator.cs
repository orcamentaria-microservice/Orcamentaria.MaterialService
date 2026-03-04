using FluentValidation;
using FluentValidation.Results;
using Orcamentaria.Lib.Domain.Enums;
using Orcamentaria.Lib.Domain.Exceptions;
using Orcamentaria.MaterialService.Domain.Models;
using Orcamentaria.MaterialService.Domain.Repositories;
using Orcamentaria.MaterialService.Domain.ServiceClient;
using Orcamentaria.MaterialService.Domain.Services;
using Orcamentaria.MaterialService.Domain.Validators;
using System.Linq;

namespace Orcamentaria.MaterialService.Application.Validators
{
    public class MaterialValidator : AbstractValidator<Material>, IMaterialValidator
    {
        private readonly IMaterialRepository<Material> _repository;
        private readonly IMaterialTypeService _materialTypeService;
        private readonly IPersonServiceClient _personServiceClient;

        public MaterialValidator(
            IMaterialRepository<Material> repository,
            IMaterialTypeService materialTypeService,
            IPersonServiceClient personServiceClient)
        {
            _repository = repository;
            _materialTypeService = materialTypeService;
            _personServiceClient = personServiceClient;
        }

        public void CommonValidation(Material entity) 
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("O {PropertyName} é obrigatório.")
                .MaximumLength(60).WithMessage("O tamanho máximo do {PropertyName} é de {MaxLength} caracteres.");
            RuleFor(x => x.Description)
                .MaximumLength(256).WithMessage("O tamanho máximo do {PropertyName} é de {MaxLength} caracteres.");
            RuleFor(x => x.Manufacturer)
                .MaximumLength(150).WithMessage("O tamanho máximo do {PropertyName} é de {MaxLength} caracteres.");
            RuleFor(x => x.TypeId)
                .NotEmpty().WithMessage("O {PropertyName} é obrigatório.")
                .Must((x, cancellation) =>
                {
                    var type = _materialTypeService.GetByIdAsync(x.TypeId).GetAwaiter().GetResult();
                    return type is not null;
                }).WithMessage("O tipo informado não existe.");
        }

        public ValidationResult ValidateBeforeInsert(Material entity)
        {
            CommonValidation(entity);

            RuleFor(x => x.Id)
                .Empty().WithMessage("O {PropertyName} não deve ser informado.");

            return Validate(entity);
        }

        public ValidationResult ValidateBeforeUpdate(Material entity)
        {
            CommonValidation(entity);

            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("O {PropertyName} deve ser informado.");

            RuleFor(x => x.Id)
               .Must((x, cancelation) =>
               {
                   var entity = _repository.GetByIdAsync(x.Id).GetAwaiter().GetResult();

                   return entity is not null;

               }).WithMessage("Id não encontrado.");

            return Validate(entity);
        }

        public ValidationResult ValidateSuppliersAsync(IEnumerable<long> suppliers)
        {
            try
            {
                var response = _personServiceClient.GetSuppliersAsync(suppliers).GetAwaiter().GetResult();

                RuleFor(_ => suppliers)
                     .Custom((suppliersIds, ctx) =>
                     {
                        string message = response.Message ?? "";

                        if (!response.Success && message.ToLowerInvariant().Contains("fallback"))
                             ctx.AddFailure("Suppliers", $"Falha externa ao validar fornecedores. (fallback)");
                    });

                var validate = Validate(new Material());
                if (!validate.IsValid)
                    return validate;

                RuleFor(_ => suppliers)
                     .Custom((suppliersIds, ctx) =>
                     {
                         if (response.Data is null && response.Error?.ErrorCode == ErrorCodeEnum.NotFound)
                             ctx.AddFailure("Todos os fornecedores são inválidos");
                     });

                validate = Validate(new Material());
                if (!validate.IsValid)
                    return validate;

                RuleFor(_ => suppliers)
                    .Custom((suppliersIds, ctx) =>
                    {
                        var validIds = new HashSet<long>(response.Data.Select(p => p.Id));
                        var invalid = suppliersIds.Where(id => !validIds.Contains(id)).ToList();

                        if (invalid.Count > 0)
                            ctx.AddFailure("Suppliers", $"Fornecedores inválidos: {string.Join(", ", invalid)}");
                    });

                return Validate(new Material());
            }
            catch (Exception ex)
            {
                throw new UnexpectedException($"Erro inesperado os validar dados dos fornecedor :{ex.Message}", ex);
            }
        }
    }
}
