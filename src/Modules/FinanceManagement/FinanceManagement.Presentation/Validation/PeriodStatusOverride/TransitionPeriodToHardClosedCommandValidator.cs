using FinanceManagement.Application.PeriodStatusOverride.Commands.TransitionPeriodToHardClosed;
using FluentValidation;

namespace FinanceManagement.Presentation.Validation.PeriodStatusOverride
{
    public class TransitionPeriodToHardClosedCommandValidator : AbstractValidator<TransitionPeriodToHardClosedCommand>
    {
        public TransitionPeriodToHardClosedCommandValidator()
        {
            RuleFor(x => x.PeriodId)
                .GreaterThan(0)
                .WithMessage("Period Id is required.");
        }
    }
}
