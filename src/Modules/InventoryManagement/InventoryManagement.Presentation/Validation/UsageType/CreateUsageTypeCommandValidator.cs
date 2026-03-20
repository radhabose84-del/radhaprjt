using FluentValidation;
using InventoryManagement.Application.Common.Interfaces.IUsageType;
using InventoryManagement.Application.UsageType.Commands.CreateUsageType;
using InventoryManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace InventoryManagement.Presentation.Validation.UsageType
{
    public class CreateUsageTypeCommandValidator : AbstractValidator<CreateUsageTypeCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IUsageTypeQueryRepository _queryRepo;

        public CreateUsageTypeCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IUsageTypeQueryRepository queryRepo)
        {
            _queryRepo = queryRepo;

            var maxLengthCode = maxLengthProvider.GetMaxLength<Domain.Entities.UsageType>("UsageTypeCode") ?? 20;
            var maxLengthName = maxLengthProvider.GetMaxLength<Domain.Entities.UsageType>("UsageTypeName") ?? 100;
            var maxLengthDesc = maxLengthProvider.GetMaxLength<Domain.Entities.UsageType>("Description") ?? 250;

            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules == null || _validationRules.Count == 0)
            {
                throw new InvalidOperationException("Validation rules could not be loaded.");
            }

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.UsageTypeCode)
                            .NotNull()
                            .WithMessage($"{nameof(CreateUsageTypeCommand.UsageTypeCode)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateUsageTypeCommand.UsageTypeCode)} {rule.Error}");

                        RuleFor(x => x.UsageTypeName)
                            .NotNull()
                            .WithMessage($"{nameof(CreateUsageTypeCommand.UsageTypeName)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateUsageTypeCommand.UsageTypeName)} {rule.Error}");
                        break;

                    case "Alphanumeric":
                        RuleFor(x => x.UsageTypeCode)
                            .Matches(rule.Pattern)
                            .WithMessage($"{nameof(CreateUsageTypeCommand.UsageTypeCode)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.UsageTypeCode));
                        break;

                    case "MaxLength":
                        RuleFor(x => x.UsageTypeCode)
                            .MaximumLength(maxLengthCode)
                            .WithMessage($"{nameof(CreateUsageTypeCommand.UsageTypeCode)} {rule.Error} {maxLengthCode} characters.");

                        RuleFor(x => x.UsageTypeName)
                            .MaximumLength(maxLengthName)
                            .WithMessage($"{nameof(CreateUsageTypeCommand.UsageTypeName)} {rule.Error} {maxLengthName} characters.");

                        RuleFor(x => x.Description)
                            .MaximumLength(maxLengthDesc)
                            .WithMessage($"{nameof(CreateUsageTypeCommand.Description)} {rule.Error} {maxLengthDesc} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Description));
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x.UsageTypeCode)
                            .MustAsync(async (code, ct) => !await _queryRepo.AlreadyExistsAsync(code!))
                            .WithMessage($"{nameof(CreateUsageTypeCommand.UsageTypeCode)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.UsageTypeCode));
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.ModuleId)
                            .MustAsync(async (id, ct) => await _queryRepo.ModuleExistsAsync(id))
                            .WithMessage($"{nameof(CreateUsageTypeCommand.ModuleId)} {rule.Error}")
                            .When(x => x.ModuleId > 0);
                        break;

                    case "GreaterThan":
                        RuleFor(x => x.ModuleId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateUsageTypeCommand.ModuleId)} {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
