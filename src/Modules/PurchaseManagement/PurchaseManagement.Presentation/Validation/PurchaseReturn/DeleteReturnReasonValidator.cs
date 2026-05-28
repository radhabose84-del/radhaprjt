using FluentValidation;
using PurchaseManagement.Application.Common.Interfaces.IReturnReason;
using PurchaseManagement.Application.PurchaseReturn.ReturnReason.Commands.DeleteReturnReason;

namespace PurchaseManagement.Presentation.Validation.PurchaseReturn;

public sealed class DeleteReturnReasonValidator : AbstractValidator<DeleteReturnReasonCommand>
{
    public DeleteReturnReasonValidator(IReturnReasonQueryRepository queryRepo)
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("A valid Id is required.")
            .DependentRules(() =>
            {
                RuleFor(x => x.Id)
                    .MustAsync(async (id, ct) => !await queryRepo.NotFoundAsync(id))
                    .WithMessage("Return Reason not found.");

                RuleFor(x => x.Id)
                    .MustAsync(async (id, ct) => !await queryRepo.SoftDeleteValidationAsync(id))
                    .WithMessage("This master is linked with other records. You cannot delete this record.");
            });
    }
}
