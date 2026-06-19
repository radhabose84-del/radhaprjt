using Contracts.Dtos.Lookups.Users;

namespace FinanceManagement.Application.Common
{
    // Single source of truth for "the current fiscal year" — the active year whose
    // [StartDate, EndDate] contains today (latest start wins on overlap). Used by both the
    // create handler (to seed the counter) and the query repo (to show "Next No. (this FY)"),
    // so what is saved and what is displayed always agree. Resolved from IFinancialYearLookup —
    // never from IsActive alone (many years are flagged active).
    public static class FinancialYearResolver
    {
        public static FinancialYearLookupDto? ResolveCurrent(IEnumerable<FinancialYearLookupDto> years)
        {
            var today = DateTime.UtcNow.Date;
            return years
                .Where(y => y.IsActive && y.StartDate.Date <= today && today <= y.EndDate.Date)
                .OrderByDescending(y => y.StartDate)
                .FirstOrDefault();
        }
    }
}
