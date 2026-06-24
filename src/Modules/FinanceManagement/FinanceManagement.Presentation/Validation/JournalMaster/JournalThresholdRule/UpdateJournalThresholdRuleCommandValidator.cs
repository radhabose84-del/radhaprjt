using FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournalThresholdRule;
using FinanceManagement.Application.JournalMaster.JournalThresholdRule.Commands.UpdateJournalThresholdRule;
using FluentValidation;

namespace FinanceManagement.Presentation.Validation.JournalMaster.JournalThresholdRule
{
    public class UpdateJournalThresholdRuleCommandValidator : AbstractValidator<UpdateJournalThresholdRuleCommand>
    {
        private readonly IJournalThresholdRuleQueryRepository _queryRepository;

        public UpdateJournalThresholdRuleCommandValidator(IJournalThresholdRuleQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Valid Id is required.")
                .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                .WithMessage("Journal threshold rule not found.")
                .When(x => x.Id > 0);

            RuleFor(x => x.RuleTypeId)
                .GreaterThan(0).WithMessage("Rule type is required.");

            RuleFor(x => x.RuleTypeId)
                .MustAsync(async (id, ct) => await _queryRepository.RuleTypeExistsAsync(id))
                .WithMessage("Rule type is invalid.")
                .When(x => x.RuleTypeId > 0);

            RuleFor(x => x.ThresholdValue!.Value)
                .GreaterThanOrEqualTo(0).WithMessage("Threshold value cannot be negative.")
                .When(x => x.ThresholdValue.HasValue);

            RuleFor(x => x.IsActive)
                .InclusiveBetween(0, 1).WithMessage("IsActive must be either 0 or 1.");
        }
    }
}
