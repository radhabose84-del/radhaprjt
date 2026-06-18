namespace FinanceManagement.Application.ScheduleIII.Dto
{
    public class ScheduleIIISubTotalFormulaDto
    {
        public int Id { get; set; }
        public int SubTotalId { get; set; }
        public int OperandTypeId { get; set; }
        public string? OperandTypeName { get; set; }   // MiscMaster JOIN
        public int? SectionItemId { get; set; }        // the operand line (set when LINEITEM)
        public int? OperandSubTotalId { get; set; }    // the operand sub-total (set when SUBTOTAL)
        public string? OperandName { get; set; }       // resolved line / sub-total name
        public int OperatorId { get; set; }
        public string? OperatorName { get; set; }      // MiscMaster JOIN (+ / −)
        public int DisplayOrder { get; set; }
    }
}
