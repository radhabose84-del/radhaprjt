namespace FinanceManagement.Application.Common.Interfaces.JournalMaster.IBackdateEnforcement
{
    /// <summary>
    /// US-GL03-04 — outcome of a backdate evaluation. The future posting handler (GL-01 FR-009)
    /// inspects this and decides:
    ///   * If RejectMessage is non-null  → return 400 with that text
    ///   * If RequiresReason is true     → stamp BackdateReason / BackdateAcknowledgedBy / …At
    ///   * Otherwise                     → just save normally (DB computed column fills IsBackdated)
    /// </summary>
    public sealed class BackdateDecision
    {
        /// <summary>True when VoucherDate &lt; today (regardless of period status).</summary>
        public bool IsBackdated { get; init; }

        /// <summary>True only when posting to a SoftClosed prior period — caller MUST stamp a reason.</summary>
        public bool RequiresReason { get; init; }

        /// <summary>True when RequiresReason but the caller didn't pass a non-empty reason.</summary>
        public bool ReasonMissing { get; init; }

        /// <summary>
        /// Non-null when the caller should return 400. Today: only set when ReasonMissing == true.
        /// HardClosed rejection lives in US-GL03-02's IPeriodPostingGate — we don't duplicate it.
        /// </summary>
        public string? RejectMessage { get; init; }

        public static BackdateDecision Allowed(bool isBackdated, bool requiresReason) => new()
        {
            IsBackdated = isBackdated,
            RequiresReason = requiresReason
        };

        public static BackdateDecision ReasonRequired() => new()
        {
            IsBackdated = true,
            RequiresReason = true,
            ReasonMissing = true,
            RejectMessage = "Backdate reason is required when posting to a soft-closed period."
        };
    }
}
