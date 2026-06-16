namespace FinanceManagement.Application.ScheduleIII.Dto
{
    public class ScheduleIIISubTotalDto
    {
        public int Id { get; set; }
        public int StructureId { get; set; }
        public string? SubTotalName { get; set; }
        public string? FormulaExpression { get; set; }
        public bool IncludeOtherIncome { get; set; }
        public bool IsSystemDefined { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }

        public List<ScheduleIIISubTotalFormulaDto> Formulas { get; set; } = new();
    }
}
