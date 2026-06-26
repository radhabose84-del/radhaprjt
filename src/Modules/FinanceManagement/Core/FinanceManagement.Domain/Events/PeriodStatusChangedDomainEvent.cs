using MediatR;

namespace FinanceManagement.Domain.Events
{
    /// <summary>
    /// US-GL03-02 / AC#4 — fired for every successful period-status change (forward or reverse).
    /// Consumers: close checklist (US-GL03-03), audit log, posting-engine cache invalidation.
    /// </summary>
    public sealed record PeriodStatusChangedDomainEvent(
        int AccountingPeriodId,
        int CompanyId,
        int FinancialYearId,
        int FromStatusId,
        string? FromStatusCode,            // 'OPEN' / 'SOFTCLOSED' / 'HARDCLOSED'
        int ToStatusId,
        string? ToStatusCode,
        int ChangedBy,
        DateTimeOffset ChangedAt,
        bool IsReversal,
        int? OverrideId                    // populated for reversal flow; null for forward transitions
    ) : INotification;
}
