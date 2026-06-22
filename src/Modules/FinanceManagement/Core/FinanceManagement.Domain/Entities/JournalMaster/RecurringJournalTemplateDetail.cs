using FinanceManagement.Domain.Common;

namespace FinanceManagement.Domain.Entities
{
    // US-GL01-11A — template line items copied into each generated journal. Amounts may be fixed
    // or formula-driven (per the parent's amount-adjustment rule).
    public class RecurringJournalTemplateDetail : BaseEntity
    {
        public int TemplateId { get; set; }             // same-module FK -> RecurringJournalTemplateHeader
        public int LineNo { get; set; }

        public int GlAccountId { get; set; }            // same-module FK -> GlAccountMaster
        public decimal? DrAmount { get; set; }          // when rule = Fixed
        public decimal? CrAmount { get; set; }          // when rule = Fixed
        public string? AmountFormula { get; set; }      // when rule = Indexed/computed

        public int? CostCentreId { get; set; }          // same-module FK -> CostCentre
        public int? ProfitCentreId { get; set; }        // same-module FK -> ProfitCentre
        public string? LineNarration { get; set; }

        // Same-module FK navigation
        public RecurringJournalTemplateHeader? Template { get; set; }
        public GlAccountMaster? GlAccount { get; set; }
        public CostCentre? CostCentre { get; set; }
        public ProfitCentre? ProfitCentre { get; set; }
    }
}
