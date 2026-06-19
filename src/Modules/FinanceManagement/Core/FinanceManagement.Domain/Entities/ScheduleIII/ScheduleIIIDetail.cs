using FinanceManagement.Domain.Common;

namespace FinanceManagement.Domain.Entities
{
    // One included line of a structure. Unique per header on (ScheduleIIISectionItemId) and (DisplayOrder).
    public class ScheduleIIIDetail : BaseEntity, IActivityTracked
    {
        public int ScheduleIIIHeaderId { get; set; }        // same-module FK -> ScheduleIIIHeader
        public int ScheduleIIISectionId { get; set; }       // same-module FK -> ScheduleIIISection
        public int ScheduleIIISectionItemId { get; set; }   // same-module FK -> ScheduleIIISectionItem (the line)
        public int DisplayOrder { get; set; }

        // Same-module FK navigations
        public ScheduleIIIHeader? Header { get; set; }
        public ScheduleIIISection? Section { get; set; }
        public ScheduleIIISectionItem? ScheduleIIISectionItem { get; set; }
    }
}
