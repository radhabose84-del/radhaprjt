using FinanceManagement.Domain.Common;

namespace FinanceManagement.Domain.Entities
{
    public class FinancialYearMaster : BaseEntity
    {
        public int CompanyId { get; set; }
        public string? FinancialYearCode { get; set; }      // "2024-25"
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }

        // FK to Finance.MiscMaster where MiscTypeCode = 'FYS' (Open / Closed)
        public int StatusId { get; set; }

        // Strict 12-month default; flip to true for first-year / FY-calendar-change exceptions
        public bool IsTransitionYear { get; set; }

        // Same-module FK navigation
        public MiscMaster? StatusMaster { get; set; }
        public ICollection<FinancialPeriodMaster>? Periods { get; set; }
    }
}
