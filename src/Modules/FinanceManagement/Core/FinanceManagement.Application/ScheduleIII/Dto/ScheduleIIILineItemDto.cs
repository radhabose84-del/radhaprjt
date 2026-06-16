namespace FinanceManagement.Application.ScheduleIII.Dto
{
    public class ScheduleIIILineItemDto
    {
        public int Id { get; set; }
        public int StructureId { get; set; }
        public int SectionId { get; set; }
        public int? ParentLineId { get; set; }
        public string? LineCode { get; set; }
        public string? LineName { get; set; }
        public string? SubClassification { get; set; }
        public string? NoteReference { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsSplitLine { get; set; }
        public int MappedCount { get; set; }                  // usage = account groups mapped in 03B (stub 0 until 03B)
        public bool IsActive { get; set; }

        public List<ScheduleIIILineItemDto> ChildLines { get; set; } = new();
    }
}
