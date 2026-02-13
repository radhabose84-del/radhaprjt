
using PurchaseManagement.Application.Common.Interfaces.IQuotation.IQuotationEntry;
using PurchaseManagement.Domain.Entities.Quotation.QuotationEntry;
using FluentValidation;

namespace Application.Purchase.Quotations.Validation;

public class QuotationHeaderValidator : AbstractValidator<QuotationHeader>
{
    public QuotationHeaderValidator(IQuotationCommandRepository repo)
    {
        RuleFor(x => x.SupplierId).GreaterThan(0);
        RuleFor(x => x.RfqId).GreaterThan(0);

        RuleFor(x => x.QuotationNumber)
            .NotEmpty()
            .MaximumLength(40);

        // AC: ValidTill must NOT be future-dated
        RuleFor(x => x.ValidTill)
            .Must(d => d <= DateOnly.FromDateTime(DateTime.UtcNow))
            .WithMessage("Valid till date cannot be in the future.");

        //RuleFor(x => x.FreightModeId).GreaterThan(0);
        //RuleFor(x => x.PaymentTermsId).GreaterThan(0);
        //RuleFor(x => x.IncotermsId).GreaterThan(0);

        // Header totals can be recalculated; still make sure they are not negative
        RuleFor(x => x.TaxableSubtotal).GreaterThanOrEqualTo(0);
        RuleFor(x => x.GstTotal).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ItemsTotal).GreaterThanOrEqualTo(0);
        RuleFor(x => x.GrandTotal).GreaterThanOrEqualTo(0);

        // At least one detail
        RuleFor(x => x.Lines)
            .NotEmpty()
            .WithMessage("At least one item is required in quotation.");

        // No duplicate items inside the same quotation
        RuleFor(x => x)
            .Must(h => h.Lines.Select(l => l.ItemId).Distinct().Count() == h.Lines.Count)
            .WithMessage("Duplicate items found in quotation lines.");

        // Optional async duplicate prevention at validation time
        RuleFor(x => new { x.SupplierId, x.RfqId })
            .MustAsync(async (ctx, pair, ct) =>
            {
                // ‘false’ means NOT exists → valid; ‘true’ means exists → invalid
                var exists = await repo.ExistsForSupplierRfqAsync(pair.SupplierId, pair.RfqId, ct);
                return !exists;
            })
            .WithMessage("A quotation already exists for this Supplier and RFQ combination.");
    }
}
