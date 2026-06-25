using FinanceManagement.Application.Common.PeriodStatus;
using FinanceManagement.Application.PeriodStatusOverride.Commands.RequestPeriodReversal;
using FluentValidation;

namespace FinanceManagement.Presentation.Validation.PeriodStatusOverride
{
    public class RequestPeriodReversalCommandValidator : AbstractValidator<RequestPeriodReversalCommand>
    {
        public RequestPeriodReversalCommandValidator()
        {
            RuleFor(x => x.PeriodId)
                .GreaterThan(0)
                .WithMessage("Period Id is required.");

            // NotEmpty must be in its own RuleFor — otherwise the .When predicate skips it.
            RuleFor(x => x.TargetStatusCode)
                .NotEmpty()
                .WithMessage("Target Status Code is required.");

            RuleFor(x => x.TargetStatusCode)
                .Must(code =>
                    string.Equals(code, PeriodStatusConstants.Open,       StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(code, PeriodStatusConstants.SoftClosed, StringComparison.OrdinalIgnoreCase))
                .WithMessage("Target Status Code must be 'OPEN' or 'SOFTCLOSED'.")
                .When(x => !string.IsNullOrWhiteSpace(x.TargetStatusCode));

            RuleFor(x => x.RequestedReason)
                .NotEmpty()
                .WithMessage("Reason is required.")
                .MaximumLength(500)
                .WithMessage("Reason cannot exceed 500 characters.");
        }
    }
}
