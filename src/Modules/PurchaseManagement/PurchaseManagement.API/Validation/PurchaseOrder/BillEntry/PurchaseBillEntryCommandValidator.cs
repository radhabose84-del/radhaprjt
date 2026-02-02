using PurchaseManagement.Application.PurchaseOrder.BillEntry.Dto;
using FluentValidation;

namespace PurchaseManagement.API.Validation.PurchaseOrder.BillEntry;

public sealed class PurchaseBillEntryCommandValidator 
    : AbstractValidator<PurchaseBillEntryDetailDto>
{
    public PurchaseBillEntryCommandValidator()
    {
        RuleFor(x => x.ItemId).GreaterThan(0);
        RuleFor(x => x.BilledQty).GreaterThan(0);
        RuleFor(x => x.BilledRate).GreaterThan(0);
        RuleFor(x => x.TaxPercentage).GreaterThanOrEqualTo(0);
    }
}

public sealed class PurchaseBillEntryDtoValidator 
    : AbstractValidator<PurchaseBillEntryHeaderDto>
{
    public PurchaseBillEntryDtoValidator()
    {
        RuleFor(x => x.BillNumber).NotEmpty().MaximumLength(50);
        RuleFor(x => x.BillDate).NotEmpty();
        RuleFor(x => x.PartyId).GreaterThan(0);
        RuleFor(x => x.POMethodId).GreaterThan(0);
        RuleFor(x => x.POCategoryId).GreaterThan(0);

        RuleFor(x => x.Lines)
            .NotNull()
            .NotEmpty()
            .WithMessage("At least one line is required.");

        RuleForEach(x => x.Lines)
            .SetValidator(new PurchaseBillEntryCommandValidator());
    }
}
