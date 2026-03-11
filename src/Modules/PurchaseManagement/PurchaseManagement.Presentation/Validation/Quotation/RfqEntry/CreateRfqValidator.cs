using FluentValidation;
using PurchaseManagement.Application.Quotation.RfqEntry.Commands.Create;
using Contracts.Interfaces;
using PurchaseManagement.Application.Common.Interfaces;           
using PurchaseManagement.Application.Common.Interfaces.IQuotation.IRfqEntry;  

// ... same usings ...

namespace PurchaseManagement.Presentation.Validation.Quotation.RfqEntry
{
    public class CreateRfqValidator : AbstractValidator<CreateRfqCommand>
    {
        private readonly IRfqQueryRepository _read;
        private readonly IIPAddressService _ip;

        public CreateRfqValidator(IRfqQueryRepository read, IIPAddressService ip)
        {
            _read = read;
            _ip   = ip;

            // -------- Pre-normalize input --------
            RuleFor(x => x.Suppliers).Custom((sups, ctx) =>
            {
                if (sups == null) return;
                foreach (var s in sups)
                    if (s.SupplierId.HasValue && s.SupplierId.Value <= 0)
                        s.SupplierId = null;
            });

            RuleFor(x => x.InitiationTypeId).GreaterThan(0);

            // ---------- Items ----------
            RuleFor(x => x.Items)
                .NotEmpty().WithMessage("At least one item is required.");

            RuleForEach(x => x.Items).ChildRules(i =>
            {
                i.RuleFor(m => m.ItemId).GreaterThan(0);
                i.RuleFor(m => m.UomId).GreaterThan(0);
                i.RuleFor(m => m.Qty).GreaterThan(0).WithName("Quantity");
            });

            RuleFor(x => x.Items)
                .Must(items => items
                    .Select(i => (i.ItemId, i.UomId))
                    .Distinct().Count() == items.Count)
                .WithMessage("Duplicate items are not allowed in the RFQ.");

            // ---------- Suppliers ----------
            RuleFor(x => x.Suppliers)
                .NotEmpty().WithMessage("At least one supplier is required.");

            RuleForEach(x => x.Suppliers).ChildRules(s =>
            {
                s.RuleFor(m => m.Name).NotEmpty();

                // Ad-hoc supplier requires Email
                s.When(m => !(m.SupplierId.HasValue && m.SupplierId.Value > 0), () =>
                {
                    s.RuleFor(m => m.Email)
                        .NotEmpty().WithMessage("Email is required for ad-hoc suppliers.")
                        .EmailAddress();
                });

                // If Email provided, ensure valid
                s.When(m => !string.IsNullOrWhiteSpace(m.Email), () =>
                {
                    s.RuleFor(m => m.Email!).EmailAddress();
                });
            });

            // No duplicate suppliers (registered by ID; ad-hoc by name+email+mobile)
            RuleFor(x => x.Suppliers)
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

                    string Key(RfqSupplierCreateDto s)
                    {
                        if (s.SupplierId.HasValue && s.SupplierId.Value > 0)
                            return $"ID:{s.SupplierId.Value}";
                        var name   = (s.Name ?? "").Trim().ToLowerInvariant();
                        var email  = NormEmail(s.Email) ?? "";
                        var mobile = NormMobile(s.Mobile) ?? "";
                        return $"SN:{name}|EM:{email}|M:{mobile}";
                    }

                    var keys = sups.Select(Key).ToList();
                    return keys.Distinct().Count() == keys.Count;
                })
                .WithMessage("Duplicate suppliers detected in request.");

            // (Optional) extra diagnostics for dup email/mobile within the same request
            RuleFor(x => x.Suppliers).Custom((sups, ctx) =>
            {
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

                for (int i = 0; i < (sups?.Count ?? 0); i++)
                {
                    var e = NormEmail(sups![i].Email);
                    if (!string.IsNullOrEmpty(e))
                    {
                        if (!emailMap.TryGetValue(e, out var lst)) emailMap[e] = lst = new List<int>();
                        lst.Add(i);
                    }

                    var m = NormMobile(sups![i].Mobile);
                    if (!string.IsNullOrEmpty(m))
                    {
                        if (!mobileMap.TryGetValue(m, out var lst)) mobileMap[m] = lst = new List<int>();
                        lst.Add(i);
                    }
                }

                var dupEmail = emailMap.FirstOrDefault(kv => kv.Value.Count > 1);
                if (!string.IsNullOrEmpty(dupEmail.Key))
                    ctx.AddFailure("Suppliers", $"Duplicate Email '{dupEmail.Key}' in request (rows: {string.Join(", ", dupEmail.Value)}).");

                var dupMobile = mobileMap.FirstOrDefault(kv => kv.Value.Count > 1);
                if (!string.IsNullOrEmpty(dupMobile.Key))
                    ctx.AddFailure("Suppliers", $"Duplicate Mobile '{dupMobile.Key}' in request (rows: {string.Join(", ", dupMobile.Value)}).");
            });

            // ---------- Block “same party × same item” before LastSubmitDate ----------
            RuleFor(x => x).CustomAsync(async (cmd, ctx, ct) =>
            {
                // Only check registered suppliers (SupplierId > 0)
                var supplierIds = cmd.Suppliers?
                    .Where(s => s.SupplierId.HasValue && s.SupplierId.Value > 0)
                    .Select(s => s.SupplierId!.Value)
                    .Distinct()
                    .ToArray() ?? Array.Empty<int>();

                var itemIds = cmd.Items?
                    .Select(i => i.ItemId)
                    .Distinct()
                    .ToArray() ?? Array.Empty<int>();

                if (supplierIds.Length == 0 || itemIds.Length == 0) return;

                var unitId = _ip.GetUnitId() ?? 0;

                
                //var today = DateOnly.FromDateTime(DateTime.Today);
                 var ist = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");
                var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, ist).Date);

                var conflicts = await _read.FindBlockingSupplierItemPairsAsync(
                    itemIds, supplierIds, unitId, today, excludingRfqId: null, ct);

                foreach (var c in conflicts)
                {
                    ctx.AddFailure(
                        propertyName: "Suppliers",
                        errorMessage: $"Cannot request Item {c.ItemId} again from Supplier {c.SupplierId} " +
                                      $"before last submission date. Existing RFQ '{c.RfqCode}' valid until " +
                                      $"{c.LastSubmitDate:yyyy-MM-dd}.");
                }
            });
        }
    }
}
