using PurchaseManagement.Domain.Entities.Quotation.QuotationEntry;
using FluentValidation;

namespace Application.Purchase.Quotations.Validation;

public class QuotationDetailValidator : AbstractValidator<QuotationDetail>
{
    public QuotationDetailValidator()
    {
        RuleFor(x => x.ItemId).GreaterThan(0);        
        RuleFor(x => x.UomId).GreaterThan(0);

        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.Rate).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Discount).GreaterThanOrEqualTo(0);

        RuleFor(x => x.GstPercent)
            .InclusiveBetween(0, 100);

        RuleFor(x => x.Warranty).GreaterThanOrEqualTo(0);

        // Totals can be recomputed server-side; if you persist snapshots, keep them non-negative
        RuleFor(x => x.LineSubtotal).GreaterThanOrEqualTo(0);
        RuleFor(x => x.GstAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Total).GreaterThanOrEqualTo(0);
    }
}
