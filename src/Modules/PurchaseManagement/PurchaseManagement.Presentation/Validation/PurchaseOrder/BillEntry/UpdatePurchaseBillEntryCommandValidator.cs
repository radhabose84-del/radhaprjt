using PurchaseManagement.Application.PurchaseOrder.BillEntry.Commands.Update;
using FluentValidation;

namespace PurchaseManagement.Presentation.Validation.PurchaseOrder.BillEntry;
public sealed class UpdatePurchaseBillEntryCommandValidator
    : AbstractValidator<UpdatePurchaseBillEntryCommand>
{
    public UpdatePurchaseBillEntryCommandValidator()
    {
        RuleFor(x => x.Data)
            .NotNull()
            .SetValidator(new PurchaseBillEntryDtoValidator());

        RuleFor(x => x.Data.Id)
            .NotNull()
            .GreaterThan(0);
    }
}