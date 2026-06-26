namespace FinanceManagement.Application.Common.Interfaces.IPeriodStatusOverride
{
    /// <summary>
    /// US-GL03-02 — checks whether a posting is allowed against a given period for the current user.
    /// Consumed by the future posting engine (GL-01 FR-009) before every commit.
    /// </summary>
    public interface IPeriodPostingGate
    {
        /// <summary>
        /// Returns null if posting is permitted; otherwise an error message suitable for surfacing
        /// to the caller (mapped to 400/403 by the posting middleware).
        /// </summary>
        Task<string?> CheckPostingAllowedAsync(int financialPeriodId, int companyId, CancellationToken ct);
    }
}
