namespace FinanceManagement.Application.ScheduleIII.Dto
{
    public class ScheduleIIISectionItemDto
    {
        public int Id { get; set; }
        public int SectionId { get; set; }
        public string? SectionName { get; set; }     // resolved via JOIN (list view)
        public string? LineCode { get; set; }
        public string? LineName { get; set; }
        public string? NoteReference { get; set; }
        public bool IsSplitLine { get; set; }
        public int MappedCount { get; set; }                  // usage = account groups mapped in 03B (stub 0 until 03B)
        public bool IsActive { get; set; }
    }
}
