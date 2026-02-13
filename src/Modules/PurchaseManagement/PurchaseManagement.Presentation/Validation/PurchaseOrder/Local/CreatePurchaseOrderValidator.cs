using FluentValidation;
using PurchaseManagement.Application.PurchaseOrder.Dtos.Local;

namespace PurchaseManagement.Presentation.Validation.PurchaseOrder.Local;

public class CreatePurchaseOrderValidator : AbstractValidator<PurchaseOrderCreateDto>
{
    public CreatePurchaseOrderValidator()
    {
        RuleFor(x => x.UnitId).GreaterThan(0);
        RuleFor(x => x.PONumber).MaximumLength(30);
        RuleFor(x => x.PODate).LessThanOrEqualTo(DateTime.Today.AddDays(1));
        RuleFor(x => x.VendorId).GreaterThan(0);
        RuleFor(x => x.CurrencyId).GreaterThan(0);

        RuleFor(x => x.Headers).NotNull().Must(h => h!.Count > 0)
            .WithMessage("At least one local header is required.");

        RuleForEach(x => x.Headers).ChildRules(h =>
        {
            h.RuleFor(x => x.Details).NotNull().Must(d => d!.Count > 0)
                .WithMessage("Each header must have at least one material line item.");

            h.RuleForEach(x => x.Details).ChildRules(d =>
            {
                d.RuleFor(z => z.ItemId).GreaterThan(0);
                d.RuleFor(z => z.Quantity).GreaterThan(0);
                d.RuleFor(z => z.UnitPrice).GreaterThanOrEqualTo(0);
                d.RuleFor(z => z.ItemValue).GreaterThanOrEqualTo(0);
            });
        });
    }
}
