using FluentValidation;

namespace PurchaseManagement.Application.PriceMaster.Commands.Create
{
    public sealed class CreatePriceMasterCommandValidator 
        : AbstractValidator<CreatePriceMasterCommand>
    {
        public CreatePriceMasterCommandValidator()
        {
            RuleFor(x => x.Data).NotNull();

            When(x => x.Data is not null, () =>
            {
                RuleFor(x => x.Data.ItemId)
                    .GreaterThan(0);

                RuleFor(x => x.Data.VendorId)
                    .GreaterThan(0);

                // Optional: only keep if UomId exists on your create DTO
                RuleFor(x => x.Data.UomId)
                    .GreaterThan(0)
                    .WithMessage("UomId must be greater than 0.");
                RuleFor(x => x.Data.ValidFrom)
                    .NotEmpty(); // DateOnly default -> 0001-01-01 is considered empty by FV

                // ✅ Avoids DateOnly vs DateOnly? operator issue
                RuleFor(x => x.Data)
                    .Must(d => !d.ValidTo.HasValue || d.ValidTo.Value >= d.ValidFrom)
                    .WithMessage("'ValidTo' must be on or after 'ValidFrom' (or null).");    

              // var today = DateOnly.FromDateTime(DateTime.Today);

                // RuleFor(x => x.Data.ValidFrom)
                //     .NotEmpty()
                //     .Must(d => d >= today)
                //     .WithMessage($"'ValidFrom' must be today or a future date ({today:yyyy-MM-dd}).");

                // RuleFor(x => x.Data)
                //     .Must(d => !d.ValidTo.HasValue || d.ValidTo.Value >= d.ValidFrom)
                //     .WithMessage("'ValidTo' must be on or after 'ValidFrom' (or null).");   



                RuleFor(x => x.Data.Details)
                    .NotNull().WithMessage("Details are required.")
                    .NotEmpty().WithMessage("At least one detail tier is required.");

                RuleForEach(x => x.Data.Details).ChildRules(t =>
                {
                    t.RuleFor(r => r.ScaleQtyFrom)
                        .GreaterThanOrEqualTo(1);

                    t.RuleFor(r => r.UnitPrice)
                        .GreaterThan(0);
                    t.RuleFor(r => r.CurrencyId)
                        .GreaterThan(0);

                    t.RuleFor(r => r)
                        .Must(r => !r.ScaleQtyTo.HasValue || r.ScaleQtyTo.Value >= r.ScaleQtyFrom)
                        .WithMessage(r => $"ScaleQtyTo must be >= ScaleQtyFrom (or null) for tier starting at {r.ScaleQtyFrom}.");
                });

                // Overlap/gap validation (no overlaps; gaps allowed)
                RuleFor(x => x.Data.Details).Custom((tiers, ctx) =>
                {
                    if (tiers == null) return;

                    var ordered = tiers
                        .OrderBy(t => t.ScaleQtyFrom)
                        .ThenBy(t => t.ScaleQtyTo ?? decimal.MaxValue)
                        .ToList();

                    for (int i = 1; i < ordered.Count; i++)
                    {
                        var prev = ordered[i - 1];
                        var cur  = ordered[i];

                        var prevTo = prev.ScaleQtyTo ?? decimal.MaxValue;

                        // Disallow overlap and touching at same boundary (e.g., prevTo = 100 and curFrom = 100)
                        if (cur.ScaleQtyFrom <= prevTo)
                        {
                            ctx.AddFailure($"Detail starting at {cur.ScaleQtyFrom} overlaps the previous detail ending at {(prev.ScaleQtyTo?.ToString() ?? "∞")}.");
                        }
                    }
                });
            });
        }
    }
}
