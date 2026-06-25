using FinanceManagement.Application.PeriodStatusOverride.Commands.ApprovePeriodReversal;
using FluentValidation;

namespace FinanceManagement.Presentation.Validation.PeriodStatusOverride
{
    public class ApprovePeriodReversalCommandValidator : AbstractValidator<ApprovePeriodReversalCommand>
    {
        public ApprovePeriodReversalCommandValidator()
        {
            RuleFor(x => x.OverrideId)
                .GreaterThan(0)
                .WithMessage("Override Id is required.");

            RuleFor(x => x.Role)
                .NotEmpty()
                .WithMessage("Approver role is required.")
                .Must(r =>
                    string.Equals(r, "CFO",      StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(r, "SysAdmin", StringComparison.OrdinalIgnoreCase))
                .WithMessage("Approver role must be 'CFO' or 'SysAdmin'.")
                .When(x => !string.IsNullOrWhiteSpace(x.Role));
        }
    }
}
