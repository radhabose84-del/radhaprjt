using FinanceManagement.Domain.Common;

namespace FinanceManagement.Domain.Entities
{
    // Maps a GSTR return section to a GL account-code range. The Derived figure (from GL balances)
    // and the Expected figure (return/portal value) are entered, then compared within TolerancePercent.
    public class GstrSectionAccountLinkage : BaseEntity, IActivityTracked
    {
        // No CompanyId — it belongs to the parent GstrSectionMaster (derived via SectionMasterId).
        public int SectionMasterId { get; set; }         // same-module FK -> GstrSectionMaster

        public string? AccountRangeFrom { get; set; }     // e.g. "6100101"
        public string? AccountRangeTo { get; set; }       // e.g. "6100199"

        public decimal? DerivedValue { get; set; }        // from GL balances (entered for now — no GL balance source yet)
        public decimal ExpectedValue { get; set; }        // return / portal figure
        public decimal TolerancePercent { get; set; }     // default 1

        public GstrSectionMaster? SectionMaster { get; set; }
    }
}
