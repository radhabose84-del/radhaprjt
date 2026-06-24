using FinanceManagement.Application.JournalMaster.RecurringJournalTemplate.Commands.GenerateRecurringJournals;
using FluentValidation;

namespace FinanceManagement.Presentation.Validation.JournalMaster.RecurringJournalTemplate
{
    public class GenerateRecurringJournalsCommandValidator : AbstractValidator<GenerateRecurringJournalsCommand>
    {
        public GenerateRecurringJournalsCommandValidator()
        {
            RuleFor(x => x.BaseCurrencyId).GreaterThan(0).WithMessage("Base currency is required.");
            RuleFor(x => x.Period).NotEmpty().WithMessage("Period is required (e.g. 2026-06).");
        }
    }
}
