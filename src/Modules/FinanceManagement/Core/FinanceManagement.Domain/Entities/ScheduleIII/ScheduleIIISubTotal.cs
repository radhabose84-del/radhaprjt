using FinanceManagement.Domain.Common;

namespace FinanceManagement.Domain.Entities
{
    public class ScheduleIIISubTotal : BaseEntity, IActivityTracked
    {
        public int CompanyId { get; set; }              // structure identity (with DivisionId) — was ScheduleIIIMasterId
        public int DivisionId { get; set; }
        public int SubTotalTypeId { get; set; }         // same-module FK -> MiscMaster (S3_SUBTOTAL_TYPE) — the node name (Gross Profit / EBITDA / PBT / PAT)
        public string? FormulaExpression { get; set; }  // cached display string, derived from formulas
        public bool IncludeOtherIncome { get; set; }
        public bool IsSystemDefined { get; set; } = true;
        public int DisplayOrder { get; set; }

        // Inverse navigation (signed operands)
        public ICollection<ScheduleIIISubTotalFormula>? Formulas { get; set; }
    }
}
