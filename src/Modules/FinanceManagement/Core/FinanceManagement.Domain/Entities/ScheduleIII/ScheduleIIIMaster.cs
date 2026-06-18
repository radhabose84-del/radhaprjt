using FinanceManagement.Domain.Common;

namespace FinanceManagement.Domain.Entities
{
    // Merged master + line-membership: ONE row per included line, carrying the structure header.
    // The structure for a company/division is the set of rows sharing (CompanyId, DivisionId).
    public class ScheduleIIIMaster : BaseEntity, IActivityTracked
    {
        public int CompanyId { get; set; }              // cross-module FK (ICompanyLookup) — no DB constraint
        public int DivisionId { get; set; }             // cross-module FK -> AppData.Division (IDivisionLookup) — no DB constraint
        public int StatusId { get; set; }      // same-module FK -> Finance.MiscMaster (S3_STATUS) — header, repeats per row
        public bool TextileSplitEnabled { get; set; }   // header, repeats per row

        public int ScheduleIIISectionItemId { get; set; }  // same-module FK -> ScheduleIIISectionItem (the included line)
        public int DisplayOrder { get; set; }

        // Same-module FK navigations
        public MiscMaster? StructureStatus { get; set; }
        public ScheduleIIISectionItem? ScheduleIIISectionItem { get; set; }
    }
}
