using FluentValidation;
using PurchaseManagement.Application.Common.Interfaces.IDeliveryScoreRule;
using PurchaseManagement.Application.DeliveryScoreRule.Commands.DeleteDeliveryScoreRule;
using Shared.Validation.Common;

namespace PurchaseManagement.Presentation.Validation.DeliveryScoreRule
{
    public class DeleteDeliveryScoreRuleCommandValidator : AbstractValidator<DeleteDeliveryScoreRuleCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IDeliveryScoreRuleQueryRepository _queryRepository;

        public DeleteDeliveryScoreRuleCommandValidator(IDeliveryScoreRuleQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;
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
                        RuleFor(x => x.Id)
                            .NotEmpty()
                            .WithMessage($"{nameof(DeleteDeliveryScoreRuleCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"DeliveryScoreRule {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
