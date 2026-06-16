namespace FinanceManagement.Application.ScheduleIII.Dto
{
    public class ScheduleIIISubTotalFormulaDto
    {
        public int Id { get; set; }
        public int SubTotalId { get; set; }
        public int OperandTypeId { get; set; }
        public string? OperandTypeName { get; set; }          // MiscMaster JOIN (LineItem / SubTotal)
        public int OperandRefId { get; set; }
        public string? OperandName { get; set; }              // resolved line/sub-total label
        public int OperatorId { get; set; }
        public string? OperatorName { get; set; }             // MiscMaster JOIN (Plus / Minus)
        public int DisplayOrder { get; set; }
    }
}
