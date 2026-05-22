using FluentValidation;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IContractPOMaster;
using PurchaseManagement.Application.ContractPOMaster.Commands.Delete;

namespace PurchaseManagement.Presentation.Validation.ContractPO;

public sealed class DeleteContractPOValidator : AbstractValidator<DeleteContractPOMasterCommand>
{
    public DeleteContractPOValidator(IContractPOMasterQueryRepository queryRepo)
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("A valid Id is required.")
            .DependentRules(() =>
            {
                RuleFor(x => x.Id)
                    .MustAsync(async (id, ct) => !await queryRepo.NotFoundAsync(id, ct))
                    .WithMessage("Contract PO not found.");

                RuleFor(x => x.Id)
                    .MustAsync(async (id, ct) => !await queryRepo.SoftDeleteValidationAsync(id))
                    .WithMessage("This Contract PO is linked with other records. You cannot delete this record.");
            });
    }
}
