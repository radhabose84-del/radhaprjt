using FinanceManagement.Domain.Common;

namespace FinanceManagement.Domain.Entities
{
    // Signed operand of a sub-total formula. References a line item directly (SectionItemId).
    public class ScheduleIIISubTotalFormula : BaseEntity, IActivityTracked
    {
        public int? SubTotalId { get; set; }            // same-module FK -> ScheduleIIISubTotal.Id (nullable)
        public int OperandTypeId { get; set; }          // same-module FK -> Finance.MiscMaster (S3_OPERAND_TYPE)
        public int? SectionItemId { get; set; }         // same-module FK -> ScheduleIIISectionItem (the operand line, nullable) — replaces OperandRefId
        public int OperatorId { get; set; }             // same-module FK -> Finance.MiscMaster (S3_OPERATOR) — + / −
        public int DisplayOrder { get; set; }

        // Same-module FK navigations
        public ScheduleIIISubTotal? SubTotal { get; set; }
        public ScheduleIIISectionItem? SectionItem { get; set; }
        public MiscMaster? OperandType { get; set; }
        public MiscMaster? Operator { get; set; }
    }
}
