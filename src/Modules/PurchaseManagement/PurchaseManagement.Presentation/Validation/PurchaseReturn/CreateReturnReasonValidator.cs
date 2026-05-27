using FluentValidation;
using PurchaseManagement.Application.Common.Interfaces.IReturnReason;
using PurchaseManagement.Application.PurchaseReturn.ReturnReason.Commands.CreateReturnReason;

namespace PurchaseManagement.Presentation.Validation.PurchaseReturn;

public sealed class CreateReturnReasonValidator : AbstractValidator<CreateReturnReasonCommand>
{
    public CreateReturnReasonValidator(IReturnReasonQueryRepository queryRepo)
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Return Reason Code is required.")
            .MaximumLength(30).WithMessage("Return Reason Code cannot exceed 30 characters.")
            .Matches("^[A-Za-z0-9]+$").WithMessage("Return Reason Code must be alphanumeric only.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(150).WithMessage("Description cannot exceed 150 characters.");

        RuleFor(x => x.ReturnTypeId)
            .GreaterThan(0).WithMessage("Return Type is required.")
            .MustAsync(async (id, ct) => await queryRepo.ReturnTypeExistsAsync(id))
            .WithMessage("Invalid Return Type reference.");

        RuleFor(x => x)
            .MustAsync(async (cmd, ct) => !await queryRepo.AlreadyExistsAsync(cmd.Code, cmd.ReturnTypeId))
            .WithMessage("Return Reason Code already exists for this Return Type.")
            .When(x => !string.IsNullOrWhiteSpace(x.Code) && x.ReturnTypeId > 0);
    }
}
