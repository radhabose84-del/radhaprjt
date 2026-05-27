using FluentValidation;
using PurchaseManagement.Application.Common.Interfaces.IDeliveryScoreRule;
using PurchaseManagement.Application.DeliveryScoreRule.Commands.UpdateDeliveryScoreRule;
using PurchaseManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace PurchaseManagement.Presentation.Validation.DeliveryScoreRule
{
    public class UpdateDeliveryScoreRuleCommandValidator : AbstractValidator<UpdateDeliveryScoreRuleCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IDeliveryScoreRuleQueryRepository _queryRepo;

        public UpdateDeliveryScoreRuleCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IDeliveryScoreRuleQueryRepository queryRepo)
        {
            _queryRepo = queryRepo;

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
                    case "MaxLength":
                        RuleFor(x => x.Description)
                            .MaximumLength(maxLengthDesc)
                            .WithMessage($"{nameof(UpdateDeliveryScoreRuleCommand.Description)} {rule.Error} {maxLengthDesc} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Description));
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0).WithMessage("Valid Id is required.")
                            .MustAsync(async (id, ct) => !await _queryRepo.NotFoundAsync(id))
                            .WithMessage($"DeliveryScoreRule {rule.Error}");
                        break;

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .InclusiveBetween(0, 1)
                            .WithMessage($"{nameof(UpdateDeliveryScoreRuleCommand.IsActive)} {rule.Error}");
                        break;

                    case "GreaterThanOrEqualToZero":
                        RuleFor(x => x.DelayDaysFrom)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(UpdateDeliveryScoreRuleCommand.DelayDaysFrom)} {rule.Error}");

                        RuleFor(x => x.DelayDaysTo)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(UpdateDeliveryScoreRuleCommand.DelayDaysTo)} {rule.Error}");

                        RuleFor(x => x.Score)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(UpdateDeliveryScoreRuleCommand.Score)} {rule.Error}");

                        RuleFor(x => x.SortOrder)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(UpdateDeliveryScoreRuleCommand.SortOrder)} {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
