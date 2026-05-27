using FluentValidation;
using PurchaseManagement.Application.Common.Interfaces.IReturnReason;
using PurchaseManagement.Application.PurchaseReturn.PurchaseReturn.Commands.CreatePurchaseReturn;

namespace PurchaseManagement.Presentation.Validation.PurchaseReturn;

public sealed class CreatePurchaseReturnValidator : AbstractValidator<CreatePurchaseReturnCommand>
{
    public CreatePurchaseReturnValidator(IReturnReasonQueryRepository reasonRepo)
    {
        RuleFor(x => x.RtvDate).NotEmpty().WithMessage("RTV Date is required.");
        RuleFor(x => x.UnitId).GreaterThan(0).WithMessage("Unit is required.");
        RuleFor(x => x.VendorId).GreaterThan(0).WithMessage("Vendor is required.");
        RuleFor(x => x.PoId).GreaterThan(0).WithMessage("Purchase Order is required.");
        RuleFor(x => x.GrnHeaderId).GreaterThan(0).WithMessage("GRN is required.");
        RuleFor(x => x.ReturnTypeId).GreaterThan(0).WithMessage("Return Type is required.");
        RuleFor(x => x.ReturnReasonId).GreaterThan(0).WithMessage("Return Reason is required.");
        RuleFor(x => x.ReturnActionId).GreaterThan(0).WithMessage("Return Action is required.");

        RuleFor(x => x.Remarks)
            .MaximumLength(500).WithMessage("Remarks cannot exceed 500 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Remarks));

        // Reason must belong to the chosen Return Type
        RuleFor(x => x)
            .MustAsync(async (cmd, ct) => await reasonRepo.BelongsToReturnTypeAsync(cmd.ReturnReasonId, cmd.ReturnTypeId))
            .WithMessage("Selected Return Reason does not belong to the chosen Return Type.")
            .When(x => x.ReturnReasonId > 0 && x.ReturnTypeId > 0);

        RuleFor(x => x.Details)
            .NotNull().WithMessage("At least one return line is required.")
            .NotEmpty().WithMessage("At least one return line is required.");

        RuleForEach(x => x.Details).ChildRules(detail =>
        {
            detail.RuleFor(d => d.GrnDetailId).GreaterThan(0).WithMessage("GrnDetailId is required for each line.");
            detail.RuleFor(d => d.ItemId).GreaterThan(0).WithMessage("ItemId is required for each line.");
            detail.RuleFor(d => d.UomId).GreaterThan(0).WithMessage("UomId is required for each line.");
            detail.RuleFor(d => d.ReturnQty).GreaterThan(0).WithMessage("ReturnQty must be greater than zero.");
            detail.RuleFor(d => d.ReturnQty)
                .LessThanOrEqualTo(d => d.AcceptedQty)
                .WithMessage("ReturnQty cannot exceed AcceptedQty for the GRN line.")
                .When(d => d.AcceptedQty > 0);
        }).When(x => x.Details != null && x.Details.Count > 0);
    }
}
