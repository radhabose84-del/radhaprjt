using FinanceManagement.Domain.Common;

namespace FinanceManagement.Domain.Entities
{
    // US-GL01-11A — recurring journal template authoring (header). The generation job (11B) instantiates
    // independent journals from this template; editing the template never touches posted instances.
    public class RecurringJournalTemplateHeader : BaseEntity
    {
        public string? TemplateName { get; set; }

        public int VoucherTypeId { get; set; }              // same-module FK -> VoucherTypeMaster
        public int FrequencyId { get; set; }                // same-module FK -> MiscMaster (RECURRING_FREQUENCY)

        public DateOnly StartDate { get; set; }
        public DateOnly? EndDate { get; set; }              // NULL = open-ended

        public bool AutoPost { get; set; }
        public int AmountAdjustmentRuleId { get; set; }     // same-module FK -> MiscMaster (AMOUNT_ADJ_RULE)
        public bool LowRisk { get; set; }                   // instances may bypass approval

        // Same-module FK navigation
        public VoucherTypeMaster? VoucherType { get; set; }
        public MiscMaster? Frequency { get; set; }
        public MiscMaster? AmountAdjustmentRule { get; set; }

        // Children
        public ICollection<RecurringJournalTemplateDetail>? Lines { get; set; }
    }
}
