using FinanceManagement.Domain.Common;

namespace FinanceManagement.Domain.Entities
{
    public class ScheduleIIILineItem : BaseEntity
    {
        public int StructureId { get; set; }            // same-module FK -> ScheduleIIIStructure
        public int SectionId { get; set; }              // same-module FK -> ScheduleIIISection
        public int? ParentLineId { get; set; }          // self-FK (PPE > Tangible, Trade Payables > MSME ...)
        public string? LineCode { get; set; }
        public string? LineName { get; set; }
        public string? SubClassification { get; set; }  // "(A) MSME dues" / "(B) other than MSME"
        public string? NoteReference { get; set; }      // "Note 8"
        public int DisplayOrder { get; set; }
        public bool IsSplitLine { get; set; }           // textile traded/manufactured rows (AC4)

        // Same-module FK navigations
        public ScheduleIIIStructure? Structure { get; set; }
        public ScheduleIIISection? Section { get; set; }
        public ScheduleIIILineItem? ParentLine { get; set; }

        // Inverse navigation (child sub-lines)
        public ICollection<ScheduleIIILineItem>? ChildLines { get; set; }
    }
}
