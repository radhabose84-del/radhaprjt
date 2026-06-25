using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IPeriodStatusOverride;
using FinanceManagement.Application.Common.PeriodStatus;

namespace FinanceManagement.Infrastructure.Services
{
    /// <summary>
    /// US-GL03-02 — single posting-time guard consumed by GL-01 FR-009's middleware.
    /// Role names come from the JWT (mirrored from AppSecurity.UserRole at login). Read through
    /// IIPAddressService — consistent with the rest of BSOFT's session-data access pattern.
    /// </summary>
    public class PeriodPostingGate : IPeriodPostingGate
    {
        // Role-name string literals match the rows in AppSecurity.UserRole.
        // Scoped to this gate (the only place that references them). If admin renames a role in
        // the DB, rename the literal here.
        private static readonly string[] SoftClosePosting       = { "FinanceManager", "FinanceController", "CFO", "SysAdmin" };
        private static readonly string[] AdjustmentPeriodPosting = { "FinanceController", "CFO" };

        private readonly IPeriodStatusOverrideQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;

        public PeriodPostingGate(
            IPeriodStatusOverrideQueryRepository queryRepository,
            IIPAddressService ipAddressService)
        {
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;
        }

        public async Task<string?> CheckPostingAllowedAsync(int financialPeriodId, int companyId, CancellationToken ct)
        {
            var snap = await _queryRepository.GetPeriodSnapshotAsync(financialPeriodId, ct);
            if (snap == null) return "Period not found.";
            if (snap.CompanyId != companyId) return "Period not found for this company.";

            var statusCode = snap.StatusCode?.ToUpperInvariant() ?? string.Empty;

            // AC#1 — HARDCLOSED blocks everyone (DB trigger is the second line of defence).
            if (statusCode == PeriodStatusConstants.HardClosed)
                return "Period is hard-closed; postings forbidden.";

            // AC#2 — SOFTCLOSED only allows Finance Manager+
            if (statusCode == PeriodStatusConstants.SoftClosed)
            {
                if (!_ipAddressService.IsInAnyRole(SoftClosePosting))
                    return "Period is soft-closed; only Finance Manager or above may post.";
            }

            // Period-13 / adjustment-period guard (US-GL03-01 / AC#3) — even in OPEN state,
            // only Finance Controller / CFO may post.
            if (snap.IsAdjustmentPeriod)
            {
                if (!_ipAddressService.IsInAnyRole(AdjustmentPeriodPosting))
                    return "Period 13 (adjustment) — only Finance Controller or CFO may post.";
            }

            return null; // allowed
        }
    }
}
