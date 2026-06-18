namespace FinanceManagement.Application.ScheduleIII.Dto
{
    public class ScheduleIIISubTotalFormulaDto
    {
        public int Id { get; set; }
        public int? SubTotalId { get; set; }
        public int OperandTypeId { get; set; }
        public string? OperandTypeName { get; set; }   // MiscMaster JOIN
        public int? SectionItemId { get; set; }        // the operand line (replaces OperandRefId)
        public string? OperandName { get; set; }       // resolved line name
        public int OperatorId { get; set; }
        public string? OperatorName { get; set; }      // MiscMaster JOIN (+ / −)
        public int DisplayOrder { get; set; }
    }
}
