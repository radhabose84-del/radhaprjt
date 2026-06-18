namespace FinanceManagement.Application.ScheduleIII.Dto
{
    public class ScheduleIIISubTotalDto
    {
        public int Id { get; set; }
        public string? FormulaName { get; set; }       // "Gross Profit" / "EBITDA" / "Profit Before Tax" / "Profit After Tax"
        public string? FormulaExpression { get; set; }
        public bool IncludeOtherIncome { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }

        public List<ScheduleIIISubTotalFormulaDto> Formulas { get; set; } = new();
    }
}
