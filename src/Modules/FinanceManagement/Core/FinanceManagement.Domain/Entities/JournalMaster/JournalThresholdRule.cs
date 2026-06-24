using FinanceManagement.Domain.Common;

namespace FinanceManagement.Domain.Entities
{
    // US-GL01-16A — configurable rules the flagging engine (16B) evaluates against posted journals.
    // Each rule type is independently toggleable; thresholds are editable without a code change.
    public class JournalThresholdRule : BaseEntity
    {
        public int RuleTypeId { get; set; }             // same-module FK -> MiscMaster (THRESHOLD_RULE_TYPE)
        public decimal? ThresholdValue { get; set; }    // amount / round-number rules; NULL for boolean rules
        public bool Active { get; set; }                // independently toggleable
        public DateOnly EffectiveFrom { get; set; }

        // Same-module FK navigation
        public MiscMaster? RuleType { get; set; }
    }
}
