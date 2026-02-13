using System.Linq;
using FluentValidation;
using PurchaseManagement.Application.Common.Interfaces;                       // IIPAddressService
using PurchaseManagement.Application.Common.Interfaces.IQuotation.IRfqEntry; // IRfqQueryRepository

namespace PurchaseManagement.Presentation.Validation.Quotation.RfqEntry
{
    public class UpdateRfqValidator : AbstractValidator<UpdateRfqCommand>
    {
        public UpdateRfqValidator(IRfqQueryRepository read, IIPAddressService ip)
        {
            // Normalize SupplierId==0 -> null
            RuleFor(x => x.Suppliers).Custom((sups, ctx) =>
            {
                if (sups == null) return;
                foreach (var s in sups)
                    if (s.SupplierId.HasValue && s.SupplierId.Value <= 0)
                        s.SupplierId = null;
            });

            RuleFor(x => x.InitiationTypeId).GreaterThan(0);

            // Items
            RuleForEach(x => x.Items).ChildRules(i =>
            {
                i.RuleFor(m => m.ItemId).GreaterThan(0);
                i.RuleFor(m => m.UomId).GreaterThan(0);
                i.RuleFor(m => m.Quantity).GreaterThan(0).WithName("Quantity");
                // i.RuleFor(m => m.HsnId).GreaterThan(0); // uncomment if required
            });

            RuleFor(x => x.Items)
                .Must(items => items
                    .Select(i => (i.ItemId, i.UomId))
                    .Distinct().Count() == items.Count)
                .WithMessage("Duplicate items in request.");

            // Suppliers
            RuleFor(x => x.Suppliers).NotEmpty();

            RuleForEach(x => x.Suppliers).ChildRules(s =>
            {
                s.RuleFor(m => m.Name).NotEmpty();

                // Ad-hoc supplier requires Email
                s.When(m => !(m.SupplierId.HasValue && m.SupplierId.Value > 0), () =>
                {
                    s.RuleFor(m => m.Email).NotEmpty().EmailAddress();
                });

                // If Email provided, ensure valid
                s.When(m => !string.IsNullOrWhiteSpace(m.Email), () =>
                {
                    s.RuleFor(m => m.Email!).EmailAddress();
                });
            });

            // No duplicate supplier identity inside THIS request
            RuleFor(x => x.Suppliers)
                .Must(sups =>
                {
                    string Key(RfqSupplierUpsertDto s)
                        => (s.SupplierId.HasValue && s.SupplierId.Value > 0)
                           ? $"ID:{s.SupplierId.Value}"
                           : $"SN:{(s.Name ?? "").Trim().ToLowerInvariant()}|EM:{(s.Email ?? "").Trim().ToLowerInvariant()}|M:{(s.Mobile ?? "").Trim()}";

                    var keys = sups.Select(Key).ToList();
                    return keys.Distinct().Count() == keys.Count;
                })
                .WithMessage("Duplicate suppliers in request.");

            // Business rule: block same Supplier × Item before LastSubmitDate (DateOnly)
            RuleFor(x => x).CustomAsync(async (cmd, ctx, ct) =>
            {
                var supplierIds = cmd.Suppliers?
                    .Where(s => s.SupplierId.HasValue && s.SupplierId.Value > 0)
                    .Select(s => s.SupplierId!.Value)
                    .Distinct()
                    .ToArray() ?? System.Array.Empty<int>();

                var itemIds = cmd.Items?
                    .Select(i => i.ItemId)
                    .Distinct()
                    .ToArray() ?? System.Array.Empty<int>();

                if (supplierIds.Length == 0 || itemIds.Length == 0) return;

                var unitId = ip.GetUnitId();

                // If you need IST specifically, convert from UTC first; otherwise DateTime.Today is fine.
                var ist = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");
                var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, ist).Date);
                //var today = DateOnly.FromDateTime(System.DateTime.Today);

                // NOTE: pass 'today' as a POSITIONAL 4th argument (avoid named arg mismatch)
                var conflicts = await read.FindBlockingSupplierItemPairsAsync(
                    itemIds,
                    supplierIds,
                    unitId,
                    today,             
                    excludingRfqId: cmd.Id,
                    ct: ct);

                foreach (var c in conflicts)
                {
                    ctx.AddFailure(
                        propertyName: "Suppliers",
                        errorMessage: $"Item {c.ItemId} cannot be requested again from Supplier {c.SupplierId} " +
                                      $"before the last submission date. RFQ '{c.RfqCode}' is valid until {c.LastSubmitDate:yyyy-MM-dd}.");
                }
            });
        }
    }
}
