using FluentValidation;
using FinanceManagement.Application.CoaChangeRequest.Commands.ApproveCoaChangeImpact;

namespace FinanceManagement.Presentation.Validation.CoaChangeRequest
{
    public class ApproveCoaChangeImpactCommandValidator : AbstractValidator<ApproveCoaChangeImpactCommand>
    {
        public ApproveCoaChangeImpactCommandValidator()
        {
            RuleFor(x => x.ChangeRequestId)
                .GreaterThan(0).WithMessage("Valid change request Id is required.");
        }
    }
}
