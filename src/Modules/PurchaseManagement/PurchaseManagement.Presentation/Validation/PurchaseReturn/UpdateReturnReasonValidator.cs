using FluentValidation;
using PurchaseManagement.Application.Common.Interfaces.IReturnReason;
using PurchaseManagement.Application.PurchaseReturn.ReturnReason.Commands.UpdateReturnReason;

namespace PurchaseManagement.Presentation.Validation.PurchaseReturn;

public sealed class UpdateReturnReasonValidator : AbstractValidator<UpdateReturnReasonCommand>
{
    public UpdateReturnReasonValidator(IReturnReasonQueryRepository queryRepo)
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("A valid Id is required.")
            .MustAsync(async (id, ct) => !await queryRepo.NotFoundAsync(id))
            .WithMessage("Return Reason not found.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(150).WithMessage("Description cannot exceed 150 characters.");

        RuleFor(x => x.ReturnTypeId)
            .GreaterThan(0).WithMessage("Return Type is required.")
            .MustAsync(async (id, ct) => await queryRepo.ReturnTypeExistsAsync(id))
            .WithMessage("Invalid Return Type reference.");

        RuleFor(x => x.IsActive)
            .InclusiveBetween(0, 1).WithMessage("IsActive must be 0 or 1.");
    }
}
