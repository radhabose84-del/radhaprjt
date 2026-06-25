using FinanceManagement.Application.PeriodStatusOverride.Commands.TransitionPeriodToSoftClosed;
using FluentValidation;

namespace FinanceManagement.Presentation.Validation.PeriodStatusOverride
{
    public class TransitionPeriodToSoftClosedCommandValidator : AbstractValidator<TransitionPeriodToSoftClosedCommand>
    {
        public TransitionPeriodToSoftClosedCommandValidator()
        {
            // Period existence + current-status check is enforced inside the handler against the
            // snapshot DTO (single round-trip, atomic with the state-machine guard).
            RuleFor(x => x.PeriodId)
                .GreaterThan(0)
                .WithMessage("Period Id is required.");
        }
    }
}
