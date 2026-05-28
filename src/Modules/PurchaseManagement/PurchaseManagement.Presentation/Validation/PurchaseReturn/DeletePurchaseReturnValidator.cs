using FluentValidation;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseReturn;
using PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Commands.DeletePurchaseReturn;

namespace PurchaseManagement.Presentation.Validation.PurchaseReturn;

public sealed class DeletePurchaseReturnValidator : AbstractValidator<DeletePurchaseReturnCommand>
{
    public DeletePurchaseReturnValidator(IPurchaseReturnQueryRepository queryRepo)
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("A valid Id is required.")
            .MustAsync(async (id, ct) => !await queryRepo.NotFoundAsync(id))
            .WithMessage("Purchase Return not found.");
    }
}
