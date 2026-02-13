using System.Linq;
using InventoryManagement.Application.Item.ItemDetail.Queries.GetAllItems;
using FluentValidation;

public sealed class CreateItemVariantsCommandValidator : AbstractValidator<ItemDto>
{
    public CreateItemVariantsCommandValidator()
    {
        CascadeMode = CascadeMode.Stop;

        // Must target a template
        RuleFor(x => x.ParentItemId)
            .NotNull().WithMessage("ParentItemId is required.")
            .GreaterThan(0).WithMessage("ParentItemId must be > 0.");

        // Flat list of selections, grouped by Combo
        RuleFor(x => x.VariantValues)
            .NotNull().WithMessage("VariantValues are required.")
            .Must(v => v.Count > 0).WithMessage("At least one variant combination is required.");

        // Per-value checks
        RuleForEach(x => x.VariantValues).ChildRules(v =>
        {
            v.RuleFor(a => a.VariantAttributeId)
                .NotNull().WithMessage("VariantAttributeId is required.")
                .GreaterThan(0).WithMessage("VariantAttributeId must be > 0.");

            v.RuleFor(a => a.OptionValue)
                .Must(s => !string.IsNullOrWhiteSpace(s))
                .WithMessage("OptionValue is required.")
                .MaximumLength(100);

            v.RuleFor(a => a.Combo)
                .GreaterThanOrEqualTo(1)
                .When(a => a.Combo.HasValue);
        });

        // Cross-field checks across combos
        RuleFor(x => x).Custom((x, ctx) =>
        {
            var groups = x.VariantValues?
                .Where(v => v != null && v.VariantAttributeId.HasValue && v.VariantAttributeId.Value > 0 && !string.IsNullOrWhiteSpace(v.OptionValue))
                .GroupBy(v => v.Combo ?? 1)
                .ToList();

            if (groups == null) return;

            foreach (var g in groups)
            {
                var dup = g.GroupBy(v => v.VariantAttributeId!.Value).FirstOrDefault(gg => gg.Count() > 1);
                if (dup != null)
                    ctx.AddFailure($"Combo {g.Key}: duplicate VariantAttributeId {dup.Key}.");
            }
            if (groups.Count > 0)
            {
                // The union set of attribute rows present in the payload
                var unionIds = groups.SelectMany(g => g.Select(v => v.VariantAttributeId!.Value))
                                     .Distinct()
                                     .OrderBy(id => id)
                                     .ToList();
                var expectedCount = unionIds.Count;

                // 2) Every combo must have the same set *and count* of VariantAttributeIds
                foreach (var g in groups)
                {
                    var set = g.Select(v => v.VariantAttributeId!.Value).Distinct().OrderBy(id => id).ToList();

                    if (set.Count != expectedCount || !set.SequenceEqual(unionIds))
                    {
                        var missing = unionIds.Except(set).ToList();
                        var extra   = set.Except(unionIds).ToList();

                        if (missing.Count > 0)
                            ctx.AddFailure($"Combo {g.Key}: missing attribute row(s): {string.Join(", ", missing)}.");

                        if (extra.Count > 0)
                            ctx.AddFailure($"Combo {g.Key}: contains unexpected attribute row(s): {string.Join(", ", extra)}.");
                    }
                }

                // 3) Flag exact duplicate combos (same VariantAttributeIds + option values)
                var comboKeys = groups
                    .Select(g => string.Join("|",
                        g.OrderBy(v => v.VariantAttributeId!.Value)
                         .Select(v => $"{v.VariantAttributeId!.Value}:{v.OptionValue.Trim().ToLower()}")))
                    .ToList();

                if (comboKeys.Count != comboKeys.Distinct().Count())
                    ctx.AddFailure("Duplicate variant combinations found in payload.");
            }
        });
    }
}
