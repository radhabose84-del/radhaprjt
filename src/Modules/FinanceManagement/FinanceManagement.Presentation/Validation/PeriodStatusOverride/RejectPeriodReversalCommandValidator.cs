using FinanceManagement.Application.PeriodStatusOverride.Commands.RejectPeriodReversal;
using FluentValidation;

namespace FinanceManagement.Presentation.Validation.PeriodStatusOverride
{
    public class RejectPeriodReversalCommandValidator : AbstractValidator<RejectPeriodReversalCommand>
    {
        public RejectPeriodReversalCommandValidator()
        {
            RuleFor(x => x.OverrideId)
                .GreaterThan(0)
                .WithMessage("Override Id is required.");

            RuleFor(x => x.RejectionReason)
                .NotEmpty()
                .WithMessage("Rejection reason is required.")
                .MaximumLength(500)
                .WithMessage("Rejection reason cannot exceed 500 characters.");
        }
    }
}
