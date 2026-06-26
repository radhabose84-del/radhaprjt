namespace FinanceManagement.Application.Common.Interfaces.IPeriodStatusOverride
{
    public interface IPeriodStatusOverrideCommandRepository
    {
        Task<int> CreateAsync(Domain.Entities.PeriodStatusOverride entity, CancellationToken ct);

        Task<int> UpdateAsync(Domain.Entities.PeriodStatusOverride entity, CancellationToken ct);

        /// <summary>
        /// Transitions an AccountingPeriod's StatusId (the 3-state FPS code: OPEN/SOFTCLOSED/HARDCLOSED),
        /// updates LastStatusChangedBy/At, and (when calledFromOverrideId is set) flips the override to
        /// APPLIED + sets AppliedAt. Single transaction.
        /// </summary>
        Task<bool> ApplyPeriodStatusChangeAsync(
            int accountingPeriodId,
            int newStatusId,
            int changedBy,
            DateTimeOffset changedAt,
            int? overrideIdToMarkApplied,
            int? appliedStatusIdForOverride,
            CancellationToken ct);
    }
}
