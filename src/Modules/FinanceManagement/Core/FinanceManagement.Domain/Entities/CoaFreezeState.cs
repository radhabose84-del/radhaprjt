using FinanceManagement.Domain.Common;

namespace FinanceManagement.Domain.Entities
{
    // The COA freeze flag (US-GL02-FR-008a) — one row per company. The DB triggers on
    // GlAccountMaster / AccountGroup read IsFrozen to reject structural writes while sealed.
    // Freeze/unfreeze TRANSITIONS are driven by US-GL02-08B (dual approval); this row is the
    // enforced state + the auto-re-freeze window. Auto-re-freeze (system) re-sets IsFrozen on expiry.
    public class CoaFreezeState : BaseEntity
    {
        public int CompanyId { get; set; }

        public bool IsFrozen { get; set; }

        // The final approver who sealed the COA (CFO) — set by 08B; resolved to name/role for the banner.
        public int? FrozenByUserId { get; set; }
        public DateTimeOffset? FrozenOn { get; set; }

        // When an unfreeze window is open (set by 08B), the Hangfire auto-re-freeze job re-freezes after this.
        public DateTimeOffset? UnfreezeWindowExpiry { get; set; }
    }
}
