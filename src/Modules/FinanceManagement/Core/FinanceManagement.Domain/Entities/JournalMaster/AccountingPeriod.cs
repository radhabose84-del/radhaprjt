using FinanceManagement.Domain.Common;

namespace FinanceManagement.Domain.Entities
{
    // GL-03 — accounting calendar. ONE row per (CompanyId, FinancialYearId, PeriodNo).
    // Drives open-period validation on save/post and the period bucket for LedgerBalance.
    //
    // US-GL03-01..05 refactor (Q1=b / Q2=b / Q3=a) — this table now carries BOTH the 12 regular
    // monthly periods AND a 13th adjustment period per FY (IsAdjustmentPeriod). StatusId is
    // repointed to MiscMaster rows under a NEW MiscType 'FPS' (OPEN / SOFTCLOSED / HARDCLOSED),
    // leaving the legacy 'PERIOD_STATUS' MiscType rows untouched in the database (Q2=b).
    public class AccountingPeriod : BaseEntity
    {
        public int CompanyId { get; set; }              // cross-module FK (ICompanyLookup) — no DB constraint
        public int FinancialYearId { get; set; }        // cross-module FK (AppData.FinancialYear) — no DB constraint

        public string? PeriodName { get; set; }         // e.g. "Jun 2026" / "Adj-2026-27"
        public int PeriodNo { get; set; }               // 1..13 within the FY (13 = adjustment period)

        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }

        public int StatusId { get; set; }               // same-module FK -> MiscMaster (FPS: OPEN/SOFTCLOSED/HARDCLOSED)

        // US-GL03-05 / Q3=a — true only for the year-end adjustment period (Period 13). The closing JV
        // posts in the row where IsAdjustmentPeriod = 1 AND PeriodNo = 13. Default false for the 12
        // regular months.
        public bool IsAdjustmentPeriod { get; set; }

        // US-GL03-02 — denormalised audit-trail snapshot for fast /status reads. Updated on every
        // SoftClose / HardClose / reversal transition.
        public int? LastStatusChangedBy { get; set; }
        public DateTimeOffset? LastStatusChangedAt { get; set; }

        // Same-module FK navigation
        public MiscMaster? PeriodStatus { get; set; }
    }
}
