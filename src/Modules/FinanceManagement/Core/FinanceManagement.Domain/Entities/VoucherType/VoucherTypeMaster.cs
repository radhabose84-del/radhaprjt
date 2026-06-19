using FinanceManagement.Domain.Common;

namespace FinanceManagement.Domain.Entities
{
    // US-GL01-02 — voucher-type configuration master (standalone; does NOT touch TransactionTypeMaster).
    // Each type owns a dedicated number series and a set of allowed account types. VoucherTypeCode
    // doubles as the series prefix. The numbering engine (FR-003) consumes these settings plus the
    // per-fiscal-year counter in VoucherTypeNumberSeries. No approval/routing here (out of scope).
    public class VoucherTypeMaster : BaseEntity
    {
        public int CompanyId { get; set; }

        public string? VoucherTypeCode { get; set; }   // e.g. JV — unique per company, immutable, also the series prefix
        public string? VoucherTypeName { get; set; }   // e.g. Journal Voucher
        public int NumberPadding { get; set; }         // running-number digits, e.g. 4 -> 0001

        public bool IsSystem { get; set; }             // true = system type (JV/PV/RV/CV); cannot delete or change code

        // Same-module children
        public ICollection<VoucherTypeAccountType>? AllowedAccountTypes { get; set; }
        public ICollection<VoucherTypeNumberSeries>? NumberSeries { get; set; }
    }
}
