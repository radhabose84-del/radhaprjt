namespace FinanceManagement.Application.Common.Interfaces.JournalMaster.IBackdateEnforcement
{
    /// <summary>
    /// US-GL03-04 — decides whether a posting is a backdated entry, and (if so) whether a
    /// mandatory reason must be present. Consumed by the future posting middleware in
    /// GL-01 FR-009; for now we ship the contract + a pure unit-tested implementation so
    /// the posting handler can plug us in with one call.
    /// </summary>
    public interface IBackdateEnforcementService
    {
        Task<BackdateDecision> EvaluateAsync(
            int companyId,
            DateOnly voucherDate,
            DateOnly today,
            string? backdateReason,
            CancellationToken ct);
    }
}
