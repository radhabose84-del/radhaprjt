#nullable disable

using FluentValidation;
using SalesManagement.Application.BusinessUnit.Commands.CreateBusinessUnit;
using SalesManagement.Application.Common.Interfaces.IBusinessUnit;
using SalesManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.BusinessUnit
{
    public class CreateBusinessUnitCommandValidator : AbstractValidator<CreateBusinessUnitCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IBusinessUnitQueryRepository _queryRepository;

        public CreateBusinessUnitCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IBusinessUnitQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

            var maxLengthCode = maxLengthProvider.GetMaxLength<SalesManagement.Domain.Entities.BusinessUnit>("BusinessUnitCode") ?? 20;
            var maxLengthName = maxLengthProvider.GetMaxLength<SalesManagement.Domain.Entities.BusinessUnit>("BusinessUnitName") ?? 100;
            var maxLengthDescription = maxLengthProvider.GetMaxLength<SalesManagement.Domain.Entities.BusinessUnit>("Description") ?? 500;

            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules == null || !_validationRules.Any())
            {
                throw new InvalidOperationException("Validation rules could not be loaded.");
            }

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.BusinessUnitCode)
                            .NotNull()
                            .WithMessage($"{nameof(CreateBusinessUnitCommand.BusinessUnitCode)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateBusinessUnitCommand.BusinessUnitCode)} {rule.Error}");

                        RuleFor(x => x.BusinessUnitName)
                            .NotNull()
                            .WithMessage($"{nameof(CreateBusinessUnitCommand.BusinessUnitName)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateBusinessUnitCommand.BusinessUnitName)} {rule.Error}");
                        break;

                    case "Alphanumeric":
                        RuleFor(x => x.BusinessUnitCode)
                            .Matches(rule.Pattern)
                            .WithMessage($"{nameof(CreateBusinessUnitCommand.BusinessUnitCode)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.BusinessUnitCode));
                        break;

                    case "MaxLength":
                        RuleFor(x => x.BusinessUnitCode)
                            .MaximumLength(maxLengthCode)
                            .WithMessage($"{nameof(CreateBusinessUnitCommand.BusinessUnitCode)} {rule.Error} {maxLengthCode} characters.");

                        RuleFor(x => x.BusinessUnitName)
                            .MaximumLength(maxLengthName)
                            .WithMessage($"{nameof(CreateBusinessUnitCommand.BusinessUnitName)} {rule.Error} {maxLengthName} characters.");

                        RuleFor(x => x.Description)
                            .MaximumLength(maxLengthDescription)
                            .WithMessage($"{nameof(CreateBusinessUnitCommand.Description)} {rule.Error} {maxLengthDescription} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Description));
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x.BusinessUnitCode)
                            .MustAsync(async (code, ct) => !await _queryRepository.AlreadyExistsAsync(code))
                            .WithMessage($"{nameof(CreateBusinessUnitCommand.BusinessUnitCode)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.BusinessUnitCode));
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
