using FluentValidation;

namespace PurchaseManagement.Application.PurchaseOrder.BillEntry.Queries.GetAll;

public sealed class GetPurchaseBillEntryListQueryValidator 
    : AbstractValidator<GetAllPurchaseBillEntryQuery>
{
    public GetPurchaseBillEntryListQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThan(0);
        RuleFor(x => x.Size).InclusiveBetween(1, 100);
    }
}
