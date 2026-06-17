using FinanceManagement.Domain.Common;

namespace FinanceManagement.Domain.Entities
{
    // US-GL02-05A AC3 — effective-dated rate versions. A rate change inserts a new row
    // (next VersionNo) and closes the prior open row; rows are never updated in place.
    public class TaxCodeRateVersion : BaseEntity, IActivityTracked
    {
        public int TaxCodeId { get; set; }              // same-module FK -> TaxCodeMaster
        public int VersionNo { get; set; }              // 1, 2, 3 ...
        public decimal RatePercent { get; set; }        // GST %, TDS rate %, or BCD %
        public DateOnly EffectiveFrom { get; set; }     // applies on/after
        public DateOnly? EffectiveTo { get; set; }      // NULL = current/open
        public string? ChangeReason { get; set; }

        // Same-module FK navigation
        public TaxCodeMaster? TaxCode { get; set; }
    }
}
