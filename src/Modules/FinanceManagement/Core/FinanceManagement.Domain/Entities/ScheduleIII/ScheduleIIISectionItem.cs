using FinanceManagement.Domain.Common;

namespace FinanceManagement.Domain.Entities
{
    // Global catalog of Schedule III line items. A master includes lines via ScheduleIIIMasterLine (junction).
    public class ScheduleIIISectionItem : BaseEntity, IActivityTracked
    {
        public int SectionId { get; set; }              // same-module FK -> ScheduleIIISection
        public string? LineCode { get; set; }
        public string? LineName { get; set; }
        public string? NoteReference { get; set; }      // "Note 8"
        public bool IsSplitLine { get; set; }           // textile traded/manufactured rows (AC4)

        // Same-module FK navigation
        public ScheduleIIISection? Section { get; set; }

        // Inverse navigation (master rows that include this line)
        public ICollection<ScheduleIIIMaster>? MasterRows { get; set; }
    }
}
