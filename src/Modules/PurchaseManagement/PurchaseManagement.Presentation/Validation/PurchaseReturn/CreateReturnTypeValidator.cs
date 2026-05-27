using FluentValidation;
using PurchaseManagement.Application.Common.Interfaces.IReturnType;
using PurchaseManagement.Application.PurchaseReturn.ReturnType.Commands.CreateReturnType;

namespace PurchaseManagement.Presentation.Validation.PurchaseReturn;

public sealed class CreateReturnTypeValidator : AbstractValidator<CreateReturnTypeCommand>
{
    public CreateReturnTypeValidator(IReturnTypeQueryRepository queryRepo)
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Return Type Code is required.")
            .MaximumLength(30).WithMessage("Return Type Code cannot exceed 30 characters.")
            .Matches("^[A-Za-z0-9]+$").WithMessage("Return Type Code must be alphanumeric only.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(100).WithMessage("Description cannot exceed 100 characters.");

        RuleFor(x => x.Code)
            .MustAsync(async (code, ct) => !await queryRepo.AlreadyExistsAsync(code))
            .WithMessage("Return Type Code already exists.")
            .When(x => !string.IsNullOrWhiteSpace(x.Code));

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
