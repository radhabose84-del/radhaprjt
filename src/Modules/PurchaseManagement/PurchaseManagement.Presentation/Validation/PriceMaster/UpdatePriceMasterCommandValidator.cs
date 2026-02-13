using System.Linq;
using PurchaseManagement.Application.Common.Interfaces.PriceMaster;
using PurchaseManagement.Domain.Common;
using FluentValidation;

namespace PurchaseManagement.Application.PriceMaster.Commands.Update
{
    public sealed class UpdatePriceMasterCommandValidator : AbstractValidator<UpdatePriceMasterCommand>
    {
        public UpdatePriceMasterCommandValidator(IPriceMasterQueryRepository qrepo)
        {
            RuleFor(x => x.Data).NotNull();

            When(x => x.Data is not null, () =>
            {
                // Basic header fields
                RuleFor(x => x.Data.Id).GreaterThan(0);
                RuleFor(x => x.Data.ItemId).GreaterThan(0);
                RuleFor(x => x.Data.VendorId).GreaterThan(0);                                
                RuleFor(x => x.Data.ValidFrom).NotEmpty();

                // DateOnly vs DateOnly? safe comparison
                // RuleFor(x => x.Data)
                //     .Must(d => !d.ValidTo.HasValue || d.ValidTo.Value >= d.ValidFrom)
                //     .WithMessage("'ValidTo' must be on or after 'ValidFrom' (or null).");  

                var today = DateOnly.FromDateTime(DateTime.Today);

                RuleFor(x => x.Data)
                    .Must(d =>
                         !BaseEntity.Status.Inactive.Equals(d.IsActive)  || // if not active, skip this rule
                        (d.ValidFrom <= today && (!d.ValidTo.HasValue || d.ValidTo.Value >= today)))
                    .WithMessage($"When IsActive is true, current date ({today:yyyy-MM-dd}) must fall between ValidFrom and ValidTo.");            

                            
                RuleFor(x => x.Data.UomId).GreaterThan(0);

                // Details required
                RuleFor(x => x.Data.Details)
                    .NotNull().WithMessage("Details are required.")
                    .NotEmpty().WithMessage("At least one detail tier is required.");

                // Per-tier validation
                RuleForEach(x => x.Data.Details).ChildRules(t =>
                {
                    t.RuleFor(r => r.ScaleQtyFrom).GreaterThanOrEqualTo(1);
                    t.RuleFor(r => r.CurrencyId).GreaterThan(0);
                    t.RuleFor(r => r.UnitPrice).GreaterThan(0);
                    t.RuleFor(r => r)
                        .Must(r => !r.ScaleQtyTo.HasValue || r.ScaleQtyTo.Value >= r.ScaleQtyFrom)
                        .WithMessage(r => $"ScaleQtyTo must be >= ScaleQtyFrom (or null) for tier starting at {r.ScaleQtyFrom}.");
                });

                // Cross-tier overlap validation (no overlaps; gaps allowed)
                RuleFor(x => x.Data.Details).Custom((tiers, ctx) =>
                {
                    if (tiers is null) return;

                    var ordered = tiers
                        .OrderBy(t => t.ScaleQtyFrom)
                        .ThenBy(t => t.ScaleQtyTo ?? decimal.MaxValue)
                        .ToList();

                    for (int i = 1; i < ordered.Count; i++)
                    {
                        var prev = ordered[i - 1];
                        var cur  = ordered[i];

                        var prevTo = prev.ScaleQtyTo ?? decimal.MaxValue;

                        // Disallow overlap and touching the same boundary
                        // If you want to ALLOW touching (e.g., 1–100 then 101–200), keep this as <= (current) and ensure your inputs follow that pattern.
                        if (cur.ScaleQtyFrom <= prevTo)
                        {
                            ctx.AddFailure($"Tier starting at {cur.ScaleQtyFrom} overlaps previous tier ending at {(prev.ScaleQtyTo?.ToString() ?? "∞")}.");
                        }
                    }
                });
            });
        }
    }
}
