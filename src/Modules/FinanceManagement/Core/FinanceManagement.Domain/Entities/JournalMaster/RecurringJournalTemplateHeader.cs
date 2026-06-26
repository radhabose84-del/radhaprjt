using FinanceManagement.Domain.Common;

namespace FinanceManagement.Domain.Entities
{
    // US-GL01-11A — recurring journal template authoring (header). The generation job (11B) instantiates
    // independent journals from this template; editing the template never touches posted instances.
    public class RecurringJournalTemplateHeader : BaseEntity
    {
        public string? TemplateName { get; set; }

        // Company + unit the auto-post job generates/posts for (set on create from the session) — an unattended
        // Hangfire job has no session, so these must live on the template. (Currency is per detail line.)
        public int CompanyId { get; set; }                  // cross-module ref — no DB FK
        public int UnitId { get; set; }                     // cross-module ref — no DB FK; stamped onto generated journals

        public int VoucherTypeId { get; set; }              // same-module FK -> VoucherTypeMaster
        public int FrequencyId { get; set; }                // same-module FK -> MiscMaster (RECURRING_FREQUENCY)

        public DateOnly StartDate { get; set; }
        public DateOnly? EndDate { get; set; }              // NULL = open-ended

        public bool AutoPost { get; set; }
        public int AmountAdjustmentRuleId { get; set; }     // same-module FK -> MiscMaster (AMOUNT_ADJ_RULE)
        public bool LowRisk { get; set; }                   // instances may bypass approval

        // Approval state of the template itself (US-GL01-11). Same-module FK -> MiscMaster (ApprovalStatus):
        // Pending / Approved / Rejected. Only an Approved + AutoPost template gets a scheduled Hangfire job.
        public int StatusId { get; set; }

        // Same-module FK navigation
        public VoucherTypeMaster? VoucherType { get; set; }
        public MiscMaster? Frequency { get; set; }
        public MiscMaster? AmountAdjustmentRule { get; set; }

        // Children
        public ICollection<RecurringJournalTemplateDetail>? Lines { get; set; }
    }
}
