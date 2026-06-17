using FinanceManagement.Domain.Common;

namespace FinanceManagement.Domain.Entities
{
    // GSTR-1 / GSTR-3B return section master. Report comes from MiscMaster (misc-type 'GSTR_REPORT').
    // Displayed in the grid as "{SectionCode} — {SectionName}" (e.g. "4A — Taxable outward supplies (B2B)").
    public class GstrSectionMaster : BaseEntity, IActivityTracked
    {
        public int CompanyId { get; set; }
        public int ReportTypeId { get; set; }            // FK -> MiscMaster (GSTR_REPORT): GSTR-1 / GSTR-3B
        public string? SectionCode { get; set; }          // e.g. "4A", "3.1(a)"
        public string? SectionName { get; set; }          // e.g. "Taxable outward supplies (B2B)"

        public MiscMaster? ReportType { get; set; }
        public ICollection<GstrSectionAccountLinkage> AccountLinkages { get; set; } = new List<GstrSectionAccountLinkage>();
    }
}
