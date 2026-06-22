namespace FinanceManagement.Application.CoaFreeze.Dto
{
    // Feeds the freeze banner + Freeze-Status panel + the 08a stat cards (US-GL02-FR-008a).
    // The dual-approval / unfreeze-requests / change-requests sections are US-GL02-08B.
    public class CoaFreezeStateDto
    {
        public bool IsFrozen { get; set; }

        public int? FrozenByUserId { get; set; }
        public string? FrozenByName { get; set; }
        public string? FrozenByRole { get; set; }   // populated by 08B (approver role); null in 08a
        public DateTimeOffset? FrozenOn { get; set; }

        public DateTimeOffset? UnfreezeWindowExpiry { get; set; }
        public DateTimeOffset? AutoReFreezeAt { get; set; }   // = UnfreezeWindowExpiry when a window is open

        public bool DbTriggerActive { get; set; }             // verified against sys.triggers

        public int TotalAccounts { get; set; }
        public int TotalAccountGroups { get; set; }
        public long BlockedAttemptsSinceFreeze { get; set; }
    }
}
