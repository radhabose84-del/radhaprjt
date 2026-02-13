using FluentValidation;
using PurchaseManagement.Application.Common.Interfaces.IPoMethodLookup;

namespace PurchaseManagement.Application.PurchaseOrder.CombinePO.Validators;

public sealed class UpdateCombinePODtoValidator : AbstractValidator<UpdateCombinePODto>
{
    public UpdateCombinePODtoValidator(IPoMethodLookup lookup)
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
                .NotNull().WithMessage("Local payload required for Local POMethod.")
                .Must(x => x!.Id > 0).WithMessage("Local.Id must be > 0.");
            RuleFor(x => x.Import).Null().WithMessage("Import payload must be null for Local POMethod.");
        });

        WhenAsync(async (x, ct) => await lookup.IsImportAsync(x.POMethodId, ct), () =>
        {
            RuleFor(x => x.Import)
                .NotNull().WithMessage("Import payload required for Import POMethod.")
                .Must(x => x!.Id > 0).WithMessage("Import.Id must be > 0.");
            RuleFor(x => x.Local).Null().WithMessage("Local payload must be null for Import POMethod.");
        });
    }
}
