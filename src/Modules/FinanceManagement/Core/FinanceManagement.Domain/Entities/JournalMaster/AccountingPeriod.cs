using FinanceManagement.Domain.Common;

namespace FinanceManagement.Domain.Entities
{
    // GL-03 — accounting calendar. ONE row per (CompanyId, FinancialYearId, PeriodNo).
    // Drives open-period validation on save/post and the period bucket for LedgerBalance.
    public class AccountingPeriod : BaseEntity
    {
        public int CompanyId { get; set; }              // cross-module FK (ICompanyLookup) — no DB constraint
        public int FinancialYearId { get; set; }        // cross-module FK (UserManagement) — no DB constraint

        public string? PeriodName { get; set; }         // e.g. "Jun 2026"
        public int PeriodNo { get; set; }               // 1..12 within the FY

        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }

        public int StatusId { get; set; }               // same-module FK -> MiscMaster (PERIOD_STATUS: OPEN/CLOSED)

        // Same-module FK navigation
        public MiscMaster? PeriodStatus { get; set; }
    }
}
