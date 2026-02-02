
using FluentValidation;
using PurchaseManagement.Application.Quotation.RfqEntry.Commands.UpsertDraft;

namespace PurchaseManagement.API.Validation.Quotation.RfqEntry
{
    public class UpsertRfqDraftValidator : AbstractValidator<UpsertRfqDraftCommand>
    {
        public UpsertRfqDraftValidator()
        {
            // Optional: Id must be > 0 when present (update mode)
            RuleFor(x => x.Id).GreaterThan(0).When(x => x.Id.HasValue);

            // -------- Normalize: SupplierId == 0 -> null (treat as ad-hoc) --------
            RuleFor(x => x.Suppliers).Custom((sups, ctx) =>
            {
                if (sups == null) return;
                foreach (var s in sups)
                    if (s.SupplierId.HasValue && s.SupplierId.Value <= 0)
                        s.SupplierId = null;
            });

            // ===================== Items (only if provided) =====================
            When(x => x.Items is not null, () =>
            {
                RuleForEach(x => x.Items!).ChildRules(i =>
                {
                    i.RuleFor(m => m.ItemId).GreaterThan(0);
                    i.RuleFor(m => m.UomId).GreaterThan(0);
                    i.RuleFor(m => m.Qty).GreaterThanOrEqualTo(0); // qty can be 0 in draft
                    // If you want to require HSN in draft, uncomment:
                    // i.RuleFor(m => m.HsnId).GreaterThan(0);
                });

                // No exact duplicate item lines (by ItemId+UomId)
                RuleFor(x => x.Items!)
                    .Must(items => items
                        .Select(i => (i.ItemId, i.UomId))
                        .Distinct().Count() == items.Count)
                    .WithMessage("Duplicate items are not allowed.");
            });

            // ===================== Suppliers (only if provided) =====================
            When(x => x.Suppliers is not null, () =>
            {
                RuleForEach(x => x.Suppliers!).ChildRules(s =>
                {
                    s.RuleFor(m => m.Name).NotEmpty(); // Email optional in draft

                    // If Email is provided, it must be valid
                    s.When(m => !string.IsNullOrWhiteSpace(m.Email), () =>
                    {
                        s.RuleFor(m => m.Email!).EmailAddress();
                    });
                });

                // No duplicate suppliers (registered by ID; ad-hoc by name+email+mobile)
                RuleFor(x => x.Suppliers!)
                    .Must(sups =>
                    {
                        static string? NormEmail(string? s) =>
                            string.IsNullOrWhiteSpace(s) ? null : s.Trim().ToLowerInvariant();

                        static string? NormMobile(string? s)
                        {
                            if (string.IsNullOrWhiteSpace(s)) return null;
                            var digits = new string(s.Where(char.IsDigit).ToArray());
                            return digits.Length > 0 ? digits : s.Trim();
                        }

                        string Key(DraftSupplierDto s)
                            => (s.SupplierId.HasValue && s.SupplierId.Value > 0)
                               ? $"ID:{s.SupplierId.Value}"
                               : $"SN:{(s.Name ?? "").Trim().ToLowerInvariant()}|EM:{NormEmail(s.Email) ?? ""}|M:{NormMobile(s.Mobile) ?? ""}";

                        var keys = sups.Select(Key).ToList();
                        return keys.Distinct().Count() == keys.Count;
                    })
                    .WithMessage("Duplicate suppliers not allowed.");

                // Extra diagnostics: duplicate Email/Mobile within the draft payload
                RuleFor(x => x.Suppliers!)
                    .Custom((sups, ctx) =>
                    {
                        if (sups == null || sups.Count == 0) return;

                        static string? NormEmail(string? s) =>
                            string.IsNullOrWhiteSpace(s) ? null : s.Trim().ToLowerInvariant();

                        static string? NormMobile(string? s)
                        {
                            if (string.IsNullOrWhiteSpace(s)) return null;
                            var digits = new string(s.Where(char.IsDigit).ToArray());
                            return digits.Length > 0 ? digits : s.Trim();
                        }

                        var emailMap  = new Dictionary<string, List<int>>();
                        var mobileMap = new Dictionary<string, List<int>>();

                        for (int i = 0; i < sups.Count; i++)
                        {
                            var e = NormEmail(sups[i].Email);
                            if (!string.IsNullOrEmpty(e))
                            {
                                if (!emailMap.TryGetValue(e, out var list)) emailMap[e] = list = new List<int>();
                                list.Add(i);
                            }

                            var m = NormMobile(sups[i].Mobile);
                            if (!string.IsNullOrEmpty(m))
                            {
                                if (!mobileMap.TryGetValue(m, out var list)) mobileMap[m] = list = new List<int>();
                                list.Add(i);
                            }
                        }

                        var dupEmail = emailMap.FirstOrDefault(kv => kv.Value.Count > 1);
                        if (!string.IsNullOrEmpty(dupEmail.Key))
                            ctx.AddFailure("Suppliers", $"Duplicate Email '{dupEmail.Key}' in draft (rows: {string.Join(", ", dupEmail.Value)}).");

                        var dupMobile = mobileMap.FirstOrDefault(kv => kv.Value.Count > 1);
                        if (!string.IsNullOrEmpty(dupMobile.Key))
                            ctx.AddFailure("Suppliers", $"Duplicate Mobile '{dupMobile.Key}' in draft (rows: {string.Join(", ", dupMobile.Value)}).");
                    });
            });
        }
    }
}
