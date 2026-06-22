using FluentValidation;
using FinanceManagement.Application.CoaChangeRequest.Commands.ApproveCoaUnfreeze;

namespace FinanceManagement.Presentation.Validation.CoaChangeRequest
{
    public class ApproveCoaUnfreezeCommandValidator : AbstractValidator<ApproveCoaUnfreezeCommand>
    {
        public ApproveCoaUnfreezeCommandValidator()
        {
            RuleFor(x => x.UnfreezeRequestId)
                .GreaterThan(0).WithMessage("Valid unfreeze request Id is required.");
        }
    }
}
