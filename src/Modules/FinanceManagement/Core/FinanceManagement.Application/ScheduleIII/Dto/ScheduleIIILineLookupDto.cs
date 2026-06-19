namespace FinanceManagement.Application.ScheduleIII.Dto
{
    // Autocomplete row for a structure's included lines (section + line item), ordered by DisplayOrder.
    public sealed class ScheduleIIILineLookupDto
    {
        public int DetailId { get; set; }
        public int DisplayOrder { get; set; }

        public int SectionId { get; set; }
        public string? SectionName { get; set; }
        public string? StatementTypeName { get; set; }   // Balance Sheet / Statement of P&L
        public string? NatureName { get; set; }          // Asset / Eq & Liab / Income / Expense

        public int ScheduleIIISectionItemId { get; set; }
        public string? LineCode { get; set; }
        public string? LineName { get; set; }
        public string? NoteReference { get; set; }
    }
}
