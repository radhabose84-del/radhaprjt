using FluentValidation;
using ProductionManagement.Application.Common.Interfaces.IYarnType;
using ProductionManagement.Application.YarnType.Commands.CreateYarnType;
using ProductionManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace ProductionManagement.Presentation.Validation.YarnType
{
    public class CreateYarnTypeCommandValidator : AbstractValidator<CreateYarnTypeCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IYarnTypeQueryRepository _queryRepo;

        public CreateYarnTypeCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IYarnTypeQueryRepository queryRepo)
        {
            _queryRepo = queryRepo;

            var maxLengthCode = maxLengthProvider.GetMaxLength<Domain.Entities.YarnType>("YarnTypeCode") ?? 20;
            var maxLengthName = maxLengthProvider.GetMaxLength<Domain.Entities.YarnType>("YarnTypeName") ?? 50;
            var maxLengthDescription = maxLengthProvider.GetMaxLength<Domain.Entities.YarnType>("Description") ?? 200;

            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules == null || _validationRules.Count == 0)
                throw new InvalidOperationException("Validation rules could not be loaded.");

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.YarnTypeCode)
                            .NotNull()
                            .WithMessage($"{nameof(CreateYarnTypeCommand.YarnTypeCode)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateYarnTypeCommand.YarnTypeCode)} {rule.Error}");

                        RuleFor(x => x.YarnTypeName)
                            .NotNull()
                            .WithMessage($"{nameof(CreateYarnTypeCommand.YarnTypeName)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateYarnTypeCommand.YarnTypeName)} {rule.Error}");
                        break;

                    case "Alphanumeric":
                        RuleFor(x => x.YarnTypeCode)
                            .Matches(rule.Pattern)
                            .WithMessage($"{nameof(CreateYarnTypeCommand.YarnTypeCode)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.YarnTypeCode));
                        break;

                    case "MaxLength":
                        RuleFor(x => x.YarnTypeCode)
                            .MaximumLength(maxLengthCode)
                            .WithMessage($"{nameof(CreateYarnTypeCommand.YarnTypeCode)} {rule.Error} {maxLengthCode} characters.");

                        RuleFor(x => x.YarnTypeName)
                            .MaximumLength(maxLengthName)
                            .WithMessage($"{nameof(CreateYarnTypeCommand.YarnTypeName)} {rule.Error} {maxLengthName} characters.");

                        RuleFor(x => x.Description)
                            .MaximumLength(maxLengthDescription)
                            .WithMessage($"{nameof(CreateYarnTypeCommand.Description)} {rule.Error} {maxLengthDescription} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Description));
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x.YarnTypeCode)
                            .MustAsync(async (code, ct) => !await _queryRepo.AlreadyExistsAsync(code!))
                            .WithMessage($"{nameof(CreateYarnTypeCommand.YarnTypeCode)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.YarnTypeCode));

                        RuleFor(x => x.YarnTypeName)
                            .MustAsync(async (name, ct) => !await _queryRepo.YarnTypeNameExistsAsync(name!))
                            .WithMessage($"{nameof(CreateYarnTypeCommand.YarnTypeName)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.YarnTypeName));
                        break;

                    case "GreaterThanOrEqualToZero":
                        RuleFor(x => x.AdditionalPrice)
                            .GreaterThanOrEqualTo(0m)
                            .WithMessage($"{nameof(CreateYarnTypeCommand.AdditionalPrice)} {rule.Error}")
                            .When(x => x.AdditionalPrice.HasValue);
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.CurrencyId)
                            .MustAsync(async (id, ct) => await _queryRepo.CurrencyExistsAsync(id!.Value, ct))
                            .WithMessage($"{nameof(CreateYarnTypeCommand.CurrencyId)} {rule.Error}")
                            .When(x => x.CurrencyId.HasValue && x.CurrencyId.Value > 0);

                        // Currency is mandatory when an Additional Price is provided
                        RuleFor(x => x.CurrencyId)
                            .Must(id => id.HasValue && id.Value > 0)
                            .WithMessage($"{nameof(CreateYarnTypeCommand.CurrencyId)} is required when {nameof(CreateYarnTypeCommand.AdditionalPrice)} is provided.")
                            .When(x => x.AdditionalPrice.HasValue && x.AdditionalPrice.Value > 0);
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
