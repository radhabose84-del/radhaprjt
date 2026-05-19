using FluentValidation;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IContractPO;
using PurchaseManagement.Application.ContractPO.Commands.Update;

namespace PurchaseManagement.Presentation.Validation.ContractPO;

public sealed class UpdateContractPOValidator : AbstractValidator<UpdateContractPOCommand>
{
    public UpdateContractPOValidator(IContractPOQueryRepository queryRepo)
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("A valid Id is required.")
            .DependentRules(() =>
            {
                RuleFor(x => x.Id)
                    .MustAsync(async (id, ct) => !await queryRepo.NotFoundAsync(id, ct))
                    .WithMessage("Contract PO not found.");
            });

        RuleFor(x => x.VendorId)
            .GreaterThan(0).WithMessage("VendorId is required.");

        RuleFor(x => x.CurrencyId)
            .GreaterThan(0).WithMessage("CurrencyId is required.");

        RuleFor(x => x.ValidityFrom)
            .NotEmpty().WithMessage("ValidityFrom is required.");

        RuleFor(x => x.ValidityTo)
            .NotEmpty().WithMessage("ValidityTo is required.")
            .GreaterThanOrEqualTo(x => x.ValidityFrom)
            .WithMessage("ValidityTo must be greater than or equal to ValidityFrom.");

        RuleFor(x => x.StatusId)
            .GreaterThan(0).WithMessage("StatusId is required.");

        RuleFor(x => x.IsActive)
            .InclusiveBetween(0, 1).WithMessage("IsActive must be either 0 or 1.");

        RuleFor(x => x.Details)
            .NotNull().WithMessage("Details are required.")
            .Must(d => d != null && d.Count > 0)
            .WithMessage("At least one detail line is required.");

        RuleForEach(x => x.Details).ChildRules(detail =>
        {
            detail.RuleFor(d => d.ItemId)
                .GreaterThan(0).WithMessage("ItemId must be greater than zero.");

            detail.RuleFor(d => d.UOMId)
                .GreaterThan(0).WithMessage("UOMId must be greater than zero.");

            detail.RuleFor(d => d.ContractQuantity)
                .GreaterThan(0).WithMessage("ContractQuantity must be greater than zero.");

            detail.RuleFor(d => d.ContractRate)
                .GreaterThan(0).WithMessage("ContractRate must be greater than zero.");
        });
    }
}
