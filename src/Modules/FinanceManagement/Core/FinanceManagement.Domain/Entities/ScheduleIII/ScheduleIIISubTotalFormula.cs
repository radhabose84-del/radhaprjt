using FinanceManagement.Domain.Common;

namespace FinanceManagement.Domain.Entities
{
    // Signed operand of a sub-total formula. The operand is either a P&L line item (SectionItemId)
    // or another sub-total (OperandSubTotalId), discriminated by OperandTypeId.
    public class ScheduleIIISubTotalFormula : BaseEntity, IActivityTracked
    {
        public int SubTotalId { get; set; }             // same-module FK -> ScheduleIIISubTotal.Id (parent formula owner, NOT NULL)
        public int OperandTypeId { get; set; }          // same-module FK -> Finance.MiscMaster (S3_OPERAND_TYPE): LINEITEM | SUBTOTAL
        public int? SectionItemId { get; set; }         // same-module FK -> ScheduleIIISectionItem (operand line — set when LINEITEM)
        public int? OperandSubTotalId { get; set; }     // same-module FK -> ScheduleIIISubTotal.Id (operand sub-total — set when SUBTOTAL)
        public int OperatorId { get; set; }             // same-module FK -> Finance.MiscMaster (S3_OPERATOR) — + / −
        public int DisplayOrder { get; set; }

        // Same-module FK navigations
        public ScheduleIIISubTotal? SubTotal { get; set; }          // parent
        public ScheduleIIISubTotal? OperandSubTotal { get; set; }   // operand (another sub-total)
        public ScheduleIIISectionItem? SectionItem { get; set; }
        public MiscMaster? OperandType { get; set; }
        public MiscMaster? Operator { get; set; }
    }
}
