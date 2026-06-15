namespace FinanceManagement.Application.ScheduleIII.Dto
{
    // One signed operand from the "Edit formula" dialog (off = absent row).
    public class SubTotalFormulaInput
    {
        public int OperandTypeId { get; set; }   // MiscMaster (S3_OPERAND_TYPE): LineItem / SubTotal
        public int OperandRefId { get; set; }     // LineItem.Id or SubTotal.Id
        public int OperatorId { get; set; }       // MiscMaster (S3_OPERATOR): Plus / Minus
        public int DisplayOrder { get; set; }
    }
}
