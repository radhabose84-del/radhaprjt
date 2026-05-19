using PurchaseManagement.Application.Common.Interfaces.IPoMethodLookup;
using FluentValidation;

namespace PurchaseManagement.Application.PurchaseOrder.CombinePO.Validators;

public sealed class CreateCombinePODtoValidator : AbstractValidator<CreateCombinePODto>
{
    public CreateCombinePODtoValidator(IPoMethodLookup lookup)
    {
        RuleFor(x => x.POMethodId)
            .MustAsync(async (id, ct) => await lookup.IsValidAsync(id, ct))
            .WithMessage("Invalid POMethodId (not found in MiscMaster).");

        RuleFor(x => x)
            .Must(x => new[] { x.Local is not null, x.Import is not null, x.Contract is not null }
                .Count(b => b) == 1)
            .WithMessage("Provide exactly one payload: 'Local', 'Import', or 'Contract'.");

        WhenAsync(async (x, ct) => await lookup.IsLocalAsync(x.POMethodId, ct), () =>
        {
            RuleFor(x => x.Local).NotNull().WithMessage("Local payload required for Local PO.");
            RuleFor(x => x.Import).Null().WithMessage("Import payload must be null for Local PO.");
            RuleFor(x => x.Contract).Null().WithMessage("Contract payload must be null for Local PO.");
        });

        WhenAsync(async (x, ct) => await lookup.IsImportAsync(x.POMethodId, ct), () =>
        {
            RuleFor(x => x.Import).NotNull().WithMessage("Import payload required for Import PO.");
            RuleFor(x => x.Local).Null().WithMessage("Local payload must be null for Import PO.");
            RuleFor(x => x.Contract).Null().WithMessage("Contract payload must be null for Import PO.");
        });

        WhenAsync(async (x, ct) => await lookup.IsContractAsync(x.POMethodId, ct), () =>
        {
            RuleFor(x => x.Contract).NotNull().WithMessage("Contract payload required for Contract PO.");
            RuleFor(x => x.Local).Null().WithMessage("Local payload must be null for Contract PO.");
            RuleFor(x => x.Import).Null().WithMessage("Import payload must be null for Contract PO.");
        });
    }
}
