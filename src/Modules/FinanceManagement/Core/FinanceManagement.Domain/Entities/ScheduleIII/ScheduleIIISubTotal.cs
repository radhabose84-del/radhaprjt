using FinanceManagement.Domain.Common;

namespace FinanceManagement.Domain.Entities
{
    // Sub-total header (global catalog of formulas).
    public class ScheduleIIISubTotal : BaseEntity, IActivityTracked
    {
        public string FormulaName { get; set; } = string.Empty;   // "Gross Profit" / "EBITDA" / "Profit Before Tax" / "Profit After Tax" — required (NOT NULL)
        public string? FormulaExpression { get; set; }  // cached display string, derived from the operand rows
        public bool IncludeOtherIncome { get; set; }
        public int DisplayOrder { get; set; }

        // Inverse navigation (signed operands)
        public ICollection<ScheduleIIISubTotalFormula>? Formulas { get; set; }
    }
}
