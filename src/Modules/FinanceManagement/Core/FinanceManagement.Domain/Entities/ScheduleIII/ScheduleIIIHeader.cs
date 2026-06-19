using FinanceManagement.Domain.Common;

namespace FinanceManagement.Domain.Entities
{
    // Structure header — ONE row per (CompanyId, DivisionId). The included lines live in ScheduleIIIDetail.
    public class ScheduleIIIHeader : BaseEntity, IActivityTracked
    {
        public int CompanyId { get; set; }              // cross-module FK (ICompanyLookup) — no DB constraint
        public int DivisionId { get; set; }             // cross-module FK -> AppData.Division (IDivisionLookup) — no DB constraint
        public int StatusId { get; set; }               // same-module FK -> Finance.MiscMaster (S3_STATUS)
        public bool TextileSplitEnabled { get; set; }

        // Same-module FK navigation
        public MiscMaster? StructureStatus { get; set; }

        // Inverse navigation — included lines
        public ICollection<ScheduleIIIDetail>? Details { get; set; }
    }
}
