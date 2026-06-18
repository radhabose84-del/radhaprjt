namespace FinanceManagement.Application.ScheduleIII.Dto
{
    public class ScheduleIIISubTotalDto
    {
        public int Id { get; set; }
        public int ScheduleIIIMasterId { get; set; }
        public int SubTotalTypeId { get; set; }
        public string? SubTotalName { get; set; }     // resolved via JOIN to MiscMaster (S3_SUBTOTAL_TYPE)
        public string? FormulaExpression { get; set; }
        public bool IncludeOtherIncome { get; set; }
        public bool IsSystemDefined { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }

        public List<ScheduleIIISubTotalFormulaDto> Formulas { get; set; } = new();
    }
}
