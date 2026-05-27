using FluentValidation;
using PurchaseManagement.Application.Common.Interfaces.IReturnType;
using PurchaseManagement.Application.PurchaseReturn.ReturnType.Commands.UpdateReturnType;

namespace PurchaseManagement.Presentation.Validation.PurchaseReturn;

public sealed class UpdateReturnTypeValidator : AbstractValidator<UpdateReturnTypeCommand>
{
    public UpdateReturnTypeValidator(IReturnTypeQueryRepository queryRepo)
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("A valid Id is required.")
            .MustAsync(async (id, ct) => !await queryRepo.NotFoundAsync(id))
            .WithMessage("Return Type not found.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(100).WithMessage("Description cannot exceed 100 characters.");

        RuleFor(x => x.IsActive)
            .InclusiveBetween(0, 1).WithMessage("IsActive must be 0 or 1.");

        RuleFor(x => x.InventoryImpactId!.Value)
            .MustAsync(async (id, ct) => await queryRepo.InventoryImpactExistsAsync(id))
            .WithMessage("Invalid Inventory Impact reference.")
            .When(x => x.InventoryImpactId.HasValue && x.InventoryImpactId.Value > 0);

        RuleFor(x => x.FinanceImpactId!.Value)
            .MustAsync(async (id, ct) => await queryRepo.FinanceImpactExistsAsync(id))
            .WithMessage("Invalid Finance Impact reference.")
            .When(x => x.FinanceImpactId.HasValue && x.FinanceImpactId.Value > 0);

        RuleFor(x => x.ApprovalRoleCode)
            .MaximumLength(30).WithMessage("Approval Role Code cannot exceed 30 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.ApprovalRoleCode));
    }
}
