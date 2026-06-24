using FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournalThresholdRule;
using FinanceManagement.Application.JournalMaster.JournalThresholdRule.Commands.CreateJournalThresholdRule;
using FluentValidation;

namespace FinanceManagement.Presentation.Validation.JournalMaster.JournalThresholdRule
{
    public class CreateJournalThresholdRuleCommandValidator : AbstractValidator<CreateJournalThresholdRuleCommand>
    {
        private readonly IJournalThresholdRuleQueryRepository _queryRepository;

        public CreateJournalThresholdRuleCommandValidator(IJournalThresholdRuleQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

            RuleFor(x => x.RuleTypeId)
                .GreaterThan(0).WithMessage("Rule type is required.");

            RuleFor(x => x.RuleTypeId)
                .MustAsync(async (id, ct) => await _queryRepository.RuleTypeExistsAsync(id))
                .WithMessage("Rule type is invalid.")
                .When(x => x.RuleTypeId > 0);

            RuleFor(x => x.ThresholdValue!.Value)
                .GreaterThanOrEqualTo(0).WithMessage("Threshold value cannot be negative.")
                .When(x => x.ThresholdValue.HasValue);
        }
    }
}
