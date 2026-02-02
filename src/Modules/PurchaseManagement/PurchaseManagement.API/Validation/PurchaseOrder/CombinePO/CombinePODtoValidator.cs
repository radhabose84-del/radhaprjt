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
            .Must(x => (x.Local is not null) ^ (x.Import is not null))
            .WithMessage("Provide exactly one payload: either 'Local' or 'Import', not both.");

        WhenAsync(async (x, ct) => await lookup.IsLocalAsync(x.POMethodId, ct), () =>
        {
            RuleFor(x => x.Local).NotNull().WithMessage("Local payload required for Local PO.");
            RuleFor(x => x.Import).Null().WithMessage("Import payload must be null for Local PO.");
        });

        WhenAsync(async (x, ct) => await lookup.IsImportAsync(x.POMethodId, ct), () =>
        {
            RuleFor(x => x.Import).NotNull().WithMessage("Import payload required for Import PO.");
            RuleFor(x => x.Local).Null().WithMessage("Local payload must be null for Import PO.");
        });
    }
}
