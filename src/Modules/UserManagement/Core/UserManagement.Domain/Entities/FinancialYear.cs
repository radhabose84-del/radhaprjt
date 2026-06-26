using UserManagement.Domain.Common;

namespace UserManagement.Domain.Entities
{
    public class  FinancialYear : BaseEntity
    {
        public int Id { get; set; }
        public string? StartYear { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public string? FinYearName { get; set; }

        // US-GL03-01 refactor (2026-06-26) — Open/Closed lifecycle. Same-module FK to
        // AppData.MiscMaster (MiscTypeCode = 'FYS' with codes OPEN / CLOSED). The 3-state period status
        // (OPEN/SOFTCLOSED/HARDCLOSED) lives on Finance.AccountingPeriod, not here.
        public int StatusId { get; set; }

        // US-GL03-01 — flips true only for the first FY or an FY-calendar-change year (the rule that
        // every FY is exactly 12 months gets relaxed for these exceptional years).
        public bool IsTransitionYear { get; set; }

        // Same-module FK navigation
        public MiscMaster? StatusMaster { get; set; }

        public CompanySettings? CompanySettings { get; set; }
    }
}