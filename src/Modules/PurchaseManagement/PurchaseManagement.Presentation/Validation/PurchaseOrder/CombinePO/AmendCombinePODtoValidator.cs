using PurchaseManagement.Application.Common.Interfaces.IPoMethodLookup;
using PurchaseManagement.Application.PurchaseOrder.CombinePO;
using FluentValidation;


namespace PurchaseManagement.Presentation.Validation.PurchaseOrder.CombinePO;

public sealed class AmendCombinePODtoValidator : AbstractValidator<AmendCombinePODto>
{
    public AmendCombinePODtoValidator(IPoMethodLookup lookup)
    {
        RuleFor(x => x.POMethodId)
            .MustAsync(async (id, ct) => await lookup.IsValidAsync(id, ct))
            .WithMessage("Invalid POMethodId (not found in MiscMaster).");

        RuleFor(x => x)
            .Must(x => (x.Local is not null) ^ (x.Import is not null))
            .WithMessage("Provide exactly one payload: either 'Local' or 'Import', not both.");

        WhenAsync(async (x, ct) => await lookup.IsLocalAsync(x.POMethodId, ct), () =>
        {
            RuleFor(x => x.Local)
                .NotNull().WithMessage("Local amendment payload required for Local POMethod.")
                .Must(x => x!.Id > 0).WithMessage("Local.Id must be > 0.");
            RuleFor(x => x.Import).Null().WithMessage("Import payload must be null for Local POMethod.");
        });

        WhenAsync(async (x, ct) => await lookup.IsImportAsync(x.POMethodId, ct), () =>
        {
            RuleFor(x => x.Import)
                .NotNull().WithMessage("Import amendment payload required for Import POMethod.")
                .Must(x => x!.Id > 0).WithMessage("Import.Id must be > 0.");
            RuleFor(x => x.Local).Null().WithMessage("Local payload must be null for Import POMethod.");
        });
    }
}
