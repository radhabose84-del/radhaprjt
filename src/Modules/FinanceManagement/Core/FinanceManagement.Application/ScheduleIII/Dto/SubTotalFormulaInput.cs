namespace FinanceManagement.Application.ScheduleIII.Dto
{
    // One signed operand from the "Edit formula" dialog (off = absent row).
    public class SubTotalFormulaInput
    {
        public int OperandTypeId { get; set; }      // MiscMaster (S3_OPERAND_TYPE): LINEITEM | SUBTOTAL
        public int? SectionItemId { get; set; }      // ScheduleIIISectionItem.Id (operand line — set when LINEITEM)
        public int? OperandSubTotalId { get; set; }  // ScheduleIIISubTotal.Id (operand sub-total — set when SUBTOTAL)
        public int OperatorId { get; set; }          // MiscMaster (S3_OPERATOR): Plus / Minus
        public int DisplayOrder { get; set; }
    }
}
