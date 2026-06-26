using Contracts.Interfaces.Lookups.Finance;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IBackdateEnforcement;
using FinanceManagement.Application.Common.PeriodStatus;

namespace FinanceManagement.Infrastructure.Services
{
    /// <summary>
    /// US-GL03-04 — decides whether a posting is backdated and (if so) whether a mandatory reason
    /// must be present. The decision matrix is the AC#2 truth table:
    ///
    ///   VoucherDate &gt;= today                    → not backdated, no reason
    ///   VoucherDate &lt;  today + period OPEN       → backdated, no reason
    ///   VoucherDate &lt;  today + period SOFTCLOSED → backdated, REASON REQUIRED
    ///   VoucherDate &lt;  today + period HARDCLOSED → still flagged backdated;
    ///                                              the actual rejection is owned by US-GL03-02's
    ///                                              IPeriodPostingGate — we don't duplicate it.
    ///   VoucherDate &lt;  today + no period found   → backdated, no reason (will be blocked later
    ///                                              by the open-period lookup in the posting handler).
    /// </summary>
    public class BackdateEnforcementService : IBackdateEnforcementService
    {
        private readonly IAccountingPeriodLookup _periodLookup;

        public BackdateEnforcementService(IAccountingPeriodLookup periodLookup)
        {
            _periodLookup = periodLookup;
        }

        public async Task<BackdateDecision> EvaluateAsync(
            int companyId,
            DateOnly voucherDate,
            DateOnly today,
            string? backdateReason,
            CancellationToken ct)
        {
            // Forward-dated or same-day → not backdated, no reason. Nothing further to check.
            if (voucherDate >= today)
                return BackdateDecision.Allowed(isBackdated: false, requiresReason: false);

            var period = await _periodLookup.GetPeriodForDateAsync(companyId, voucherDate, ct);
            var statusCode = period?.StatusCode?.ToUpperInvariant();

            // SoftClosed prior period — reason is MANDATORY. Missing → reject.
            if (statusCode == PeriodStatusConstants.SoftClosed)
            {
                if (string.IsNullOrWhiteSpace(backdateReason))
                    return BackdateDecision.ReasonRequired();

                return BackdateDecision.Allowed(isBackdated: true, requiresReason: true);
            }

            // OPEN / HARDCLOSED / not-found → backdated, no reason requirement from THIS service.
            // (HardClosed rejection belongs to IPeriodPostingGate; period-not-found belongs to the
            // posting handler's open-period resolution.)
            return BackdateDecision.Allowed(isBackdated: true, requiresReason: false);
        }
    }
}
