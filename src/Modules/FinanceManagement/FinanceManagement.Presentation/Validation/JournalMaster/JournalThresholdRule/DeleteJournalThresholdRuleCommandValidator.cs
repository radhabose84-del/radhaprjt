using FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournalThresholdRule;
using FinanceManagement.Application.JournalMaster.JournalThresholdRule.Commands.DeleteJournalThresholdRule;
using FluentValidation;
using Shared.Validation.Common;

namespace FinanceManagement.Presentation.Validation.JournalMaster.JournalThresholdRule
{
    public class DeleteJournalThresholdRuleCommandValidator : AbstractValidator<DeleteJournalThresholdRuleCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IJournalThresholdRuleQueryRepository _queryRepository;

        public DeleteJournalThresholdRuleCommandValidator(IJournalThresholdRuleQueryRepository queryRepository)
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
                        RuleFor(x => x.Id).NotEmpty().WithMessage($"{nameof(DeleteJournalThresholdRuleCommand.Id)} {rule.Error}");
                        break;
                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"Journal threshold rule {rule.Error}");
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
