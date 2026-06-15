using FinanceManagement.Domain.Common;

namespace FinanceManagement.Domain.Entities
{
    public class ScheduleIIIStructure : BaseEntity, IActivityTracked
    {
        public int CompanyId { get; set; }              // cross-module FK (ICompanyLookup) — no DB constraint
        public int DivisionId { get; set; }             // cross-module FK -> AppData.Division (IDivisionLookup) — no DB constraint
        public int StructureStatusId { get; set; }      // same-module FK -> Finance.MiscMaster (S3_STATUS)
        public bool TextileSplitEnabled { get; set; }
        public int VersionNo { get; set; } = 1;

        // Same-module FK navigation
        public MiscMaster? StructureStatus { get; set; }

        // Inverse navigations
        public ICollection<ScheduleIIISection>? Sections { get; set; }
        public ICollection<ScheduleIIILineItem>? LineItems { get; set; }
        public ICollection<ScheduleIIISubTotal>? SubTotals { get; set; }
    }
}
