using FluentValidation;
using PurchaseManagement.Application.Common.Interfaces.IDeliveryScoreRule;
using PurchaseManagement.Application.DeliveryScoreRule.Commands.CreateDeliveryScoreRule;
using PurchaseManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace PurchaseManagement.Presentation.Validation.DeliveryScoreRule
{
    public class CreateDeliveryScoreRuleCommandValidator : AbstractValidator<CreateDeliveryScoreRuleCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IDeliveryScoreRuleQueryRepository _queryRepo;

        public CreateDeliveryScoreRuleCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IDeliveryScoreRuleQueryRepository queryRepo)
        {
            _queryRepo = queryRepo;

            var maxLengthCode = maxLengthProvider.GetMaxLength<Domain.Entities.VendorEvaluation.DeliveryScoreRule>("RuleCode") ?? 20;
            var maxLengthDesc = maxLengthProvider.GetMaxLength<Domain.Entities.VendorEvaluation.DeliveryScoreRule>("Description") ?? 500;

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
                        RuleFor(x => x.RuleCode)
                            .NotNull().WithMessage($"{nameof(CreateDeliveryScoreRuleCommand.RuleCode)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateDeliveryScoreRuleCommand.RuleCode)} {rule.Error}");
                        break;

                    case "Alphanumeric":
                        RuleFor(x => x.RuleCode)
                            .Matches(rule.Pattern)
                            .WithMessage($"{nameof(CreateDeliveryScoreRuleCommand.RuleCode)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.RuleCode));
                        break;

                    case "MaxLength":
                        RuleFor(x => x.RuleCode)
                            .MaximumLength(maxLengthCode)
                            .WithMessage($"{nameof(CreateDeliveryScoreRuleCommand.RuleCode)} {rule.Error} {maxLengthCode} characters.");

                        RuleFor(x => x.Description)
                            .MaximumLength(maxLengthDesc)
                            .WithMessage($"{nameof(CreateDeliveryScoreRuleCommand.Description)} {rule.Error} {maxLengthDesc} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Description));
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x.RuleCode)
                            .MustAsync(async (code, ct) => !await _queryRepo.AlreadyExistsAsync(code!))
                            .WithMessage($"{nameof(CreateDeliveryScoreRuleCommand.RuleCode)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.RuleCode));
                        break;

                    case "GreaterThanOrEqualToZero":
                        RuleFor(x => x.DelayDaysFrom)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(CreateDeliveryScoreRuleCommand.DelayDaysFrom)} {rule.Error}");

                        RuleFor(x => x.DelayDaysTo)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(CreateDeliveryScoreRuleCommand.DelayDaysTo)} {rule.Error}");

                        RuleFor(x => x.Score)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(CreateDeliveryScoreRuleCommand.Score)} {rule.Error}");

                        RuleFor(x => x.SortOrder)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(CreateDeliveryScoreRuleCommand.SortOrder)} {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
