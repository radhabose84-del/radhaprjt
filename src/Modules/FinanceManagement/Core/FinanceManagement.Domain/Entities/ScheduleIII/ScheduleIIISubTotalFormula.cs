using FinanceManagement.Domain.Common;

namespace FinanceManagement.Domain.Entities
{
    public class ScheduleIIISubTotalFormula : BaseEntity
    {
        public int SubTotalId { get; set; }             // same-module FK -> ScheduleIIISubTotal
        public int OperandTypeId { get; set; }          // same-module FK -> Finance.MiscMaster (S3_OPERAND_TYPE)
        public int OperandRefId { get; set; }           // polymorphic ref (LineItem.Id or SubTotal.Id) — validated, no DB FK
        public int OperatorId { get; set; }             // same-module FK -> Finance.MiscMaster (S3_OPERATOR)
        public int DisplayOrder { get; set; }

        // Same-module FK navigations
        public ScheduleIIISubTotal? SubTotal { get; set; }
        public MiscMaster? OperandType { get; set; }
        public MiscMaster? Operator { get; set; }
    }
}
