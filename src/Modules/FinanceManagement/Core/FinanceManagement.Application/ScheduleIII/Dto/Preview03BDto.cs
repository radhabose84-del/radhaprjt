namespace FinanceManagement.Application.ScheduleIII.Dto
{
    // Live preview of the line options the mapping screen (US-GL02-03B) will offer.
    public class Preview03BDto
    {
        public List<Preview03BItemDto> BalanceSheetLeaves { get; set; } = new();
        public List<Preview03BItemDto> ProfitAndLossLeaves { get; set; } = new();
    }

    public class Preview03BItemDto
    {
        public int LineItemId { get; set; }
        public string? LineName { get; set; }
        public string? SectionName { get; set; }
        public string? NoteReference { get; set; }
    }
}
