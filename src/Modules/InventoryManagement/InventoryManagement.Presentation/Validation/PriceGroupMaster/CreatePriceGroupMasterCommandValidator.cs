using FluentValidation;
using InventoryManagement.Application.Common.Interfaces.IPriceGroupMaster;
using InventoryManagement.Application.PriceGroupMaster.Commands.CreatePriceGroupMaster;
using InventoryManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace InventoryManagement.Presentation.Validation.PriceGroupMaster
{
    public class CreatePriceGroupMasterCommandValidator : AbstractValidator<CreatePriceGroupMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IPriceGroupMasterQueryRepository _queryRepository;

        public CreatePriceGroupMasterCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IPriceGroupMasterQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

            var maxLengthCode = maxLengthProvider.GetMaxLength<InventoryManagement.Domain.Entities.PriceGroupMaster>("PriceGroupCode") ?? 20;
            var maxLengthName = maxLengthProvider.GetMaxLength<InventoryManagement.Domain.Entities.PriceGroupMaster>("PriceGroupName") ?? 100;
            var maxLengthDescription = maxLengthProvider.GetMaxLength<InventoryManagement.Domain.Entities.PriceGroupMaster>("Description") ?? 255;

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
                        RuleFor(x => x.PriceGroupCode)
                            .NotNull()
                            .WithMessage($"{nameof(CreatePriceGroupMasterCommand.PriceGroupCode)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(CreatePriceGroupMasterCommand.PriceGroupCode)} {rule.Error}");

                        RuleFor(x => x.PriceGroupName)
                            .NotNull()
                            .WithMessage($"{nameof(CreatePriceGroupMasterCommand.PriceGroupName)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(CreatePriceGroupMasterCommand.PriceGroupName)} {rule.Error}");

                        RuleFor(x => x.EffectiveFrom)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreatePriceGroupMasterCommand.EffectiveFrom)} {rule.Error}");
                        break;

                    case "AlphanumericWithUnderscore":
                        RuleFor(x => x.PriceGroupCode)
                            .Matches(rule.Pattern)
                            .WithMessage($"{nameof(CreatePriceGroupMasterCommand.PriceGroupCode)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.PriceGroupCode));
                        break;

                    case "MaxLength":
                        RuleFor(x => x.PriceGroupCode)
                            .MaximumLength(maxLengthCode)
                            .WithMessage($"{nameof(CreatePriceGroupMasterCommand.PriceGroupCode)} {rule.Error} {maxLengthCode} characters.");

                        RuleFor(x => x.PriceGroupName)
                            .MaximumLength(maxLengthName)
                            .WithMessage($"{nameof(CreatePriceGroupMasterCommand.PriceGroupName)} {rule.Error} {maxLengthName} characters.");

                        RuleFor(x => x.Description)
                            .MaximumLength(maxLengthDescription)
                            .WithMessage($"{nameof(CreatePriceGroupMasterCommand.Description)} {rule.Error} {maxLengthDescription} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Description));
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x.PriceGroupCode)
                            .MustAsync(async (code, ct) => !await _queryRepository.AlreadyExistsAsync(code))
                            .WithMessage($"{nameof(CreatePriceGroupMasterCommand.PriceGroupCode)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.PriceGroupCode));

                        RuleFor(x => x.PriceGroupName)
                            .MustAsync(async (name, ct) => !await _queryRepository.NameAlreadyExistsAsync(name))
                            .WithMessage($"{nameof(CreatePriceGroupMasterCommand.PriceGroupName)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.PriceGroupName));
                        break;

                    case "DateCompare":
                        RuleFor(x => x.EffectiveTo)
                            .GreaterThanOrEqualTo(x => x.EffectiveFrom)
                            .WithMessage($"{nameof(CreatePriceGroupMasterCommand.EffectiveTo)} {rule.Error} {nameof(CreatePriceGroupMasterCommand.EffectiveFrom)}.")
                            .When(x => x.EffectiveTo.HasValue);
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
