using FluentValidation;
using InventoryManagement.Application.Common.Interfaces.IUsageType;
using InventoryManagement.Application.UsageType.Commands.UpdateUsageType;
using InventoryManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace InventoryManagement.Presentation.Validation.UsageType
{
    public class UpdateUsageTypeCommandValidator : AbstractValidator<UpdateUsageTypeCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IUsageTypeQueryRepository _queryRepo;

        public UpdateUsageTypeCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IUsageTypeQueryRepository queryRepo)
        {
            _queryRepo = queryRepo;

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
                        RuleFor(x => x.UsageTypeName)
                            .NotNull()
                            .WithMessage($"{nameof(UpdateUsageTypeCommand.UsageTypeName)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateUsageTypeCommand.UsageTypeName)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.UsageTypeName)
                            .MaximumLength(maxLengthName)
                            .WithMessage($"{nameof(UpdateUsageTypeCommand.UsageTypeName)} {rule.Error} {maxLengthName} characters.");

                        RuleFor(x => x.Description)
                            .MaximumLength(maxLengthDesc)
                            .WithMessage($"{nameof(UpdateUsageTypeCommand.Description)} {rule.Error} {maxLengthDesc} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Description));
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0).WithMessage("Valid Id is required.")
                            .MustAsync(async (id, ct) => !await _queryRepo.NotFoundAsync(id))
                            .WithMessage($"UsageType {rule.Error}");
                        break;

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .InclusiveBetween(0, 1)
                            .WithMessage($"{nameof(UpdateUsageTypeCommand.IsActive)} {rule.Error}");
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.ModuleId)
                            .MustAsync(async (id, ct) => await _queryRepo.ModuleExistsAsync(id))
                            .WithMessage($"{nameof(UpdateUsageTypeCommand.ModuleId)} {rule.Error}")
                            .When(x => x.ModuleId > 0);
                        break;

                    case "GreaterThan":
                        RuleFor(x => x.ModuleId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(UpdateUsageTypeCommand.ModuleId)} {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
