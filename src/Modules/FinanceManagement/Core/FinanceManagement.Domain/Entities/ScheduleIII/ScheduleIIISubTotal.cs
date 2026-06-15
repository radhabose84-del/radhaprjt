using FinanceManagement.Domain.Common;

namespace FinanceManagement.Domain.Entities
{
    public class ScheduleIIISubTotal : BaseEntity, IActivityTracked
    {
        public int StructureId { get; set; }            // same-module FK -> ScheduleIIIStructure
        public string? SubTotalName { get; set; }
        public string? FormulaExpression { get; set; }  // cached display string, derived from formulas
        public bool IncludeOtherIncome { get; set; }
        public bool IsSystemDefined { get; set; } = true;
        public int DisplayOrder { get; set; }

        // Same-module FK navigation
        public ScheduleIIIStructure? Structure { get; set; }

        // Inverse navigation (signed operands)
        public ICollection<ScheduleIIISubTotalFormula>? Formulas { get; set; }
    }
}
