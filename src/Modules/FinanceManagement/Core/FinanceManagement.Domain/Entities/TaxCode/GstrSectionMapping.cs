using FinanceManagement.Domain.Common;

namespace FinanceManagement.Domain.Entities
{
    // US-GL02-05B AC3 — GSTR-1 / GSTR-3B section -> GL account-code range mapping.
    public class GstrSectionMapping : BaseEntity, IActivityTracked
    {
        public int CompanyId { get; set; }
        public string? GstrType { get; set; }            // GSTR1 / GSTR3B
        public string? SectionCode { get; set; }         // e.g. 3.1(a)
        public string? SectionName { get; set; }
        public string? AccountRangeFrom { get; set; }    // GL account-code range start
        public string? AccountRangeTo { get; set; }      // GL account-code range end
        public decimal? TolerancePercent { get; set; }   // derivation tolerance
    }
}
