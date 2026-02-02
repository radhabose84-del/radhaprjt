using PurchaseManagement.Application.PurchaseOrder.BillEntry.Commands.Create;
using FluentValidation;

namespace  PurchaseManagement.API.Validation.PurchaseOrder.BillEntry;

public sealed class CreatePurchaseBillEntryCommandValidator 
    : AbstractValidator<CreatePurchaseBillEntryCommand>
{
    public CreatePurchaseBillEntryCommandValidator()
    {
        RuleFor(x => x.Data)
            .NotNull()
            .SetValidator(new PurchaseBillEntryDtoValidator());
    }
}