using FinanceManagement.Domain.Common;

namespace FinanceManagement.Domain.Entities
{
    // US-GL01-02 — per-fiscal-year running-number state for a voucher type.
    // AC#2: a new fiscal year gets its own row starting at 0, so its first voucher = 1.
    // The numbering engine (FR-003) reads/increments LastUsedNumber; this master only
    // displays the counter (Number Series tab) and resets it (reset-series).
    public class VoucherTypeNumberSeries : BaseEntity
    {
        public int VoucherTypeId { get; set; }
        public int FinancialYearId { get; set; }   // cross-module (UserManagement FinancialYear) — no DB FK
        public int LastUsedNumber { get; set; }     // last consumed number; next = LastUsedNumber + 1

        // Same-module navigation
        public VoucherTypeMaster? VoucherType { get; set; }
    }
}
