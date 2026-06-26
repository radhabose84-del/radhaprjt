using System.Data;
using Contracts.Dtos.Lookups.Finance;
using Contracts.Interfaces.Lookups.Finance;
using Dapper;

namespace FinanceManagement.Infrastructure.Repositories.Lookups.Finance
{
    /// <summary>
    /// US-GL03-01..05 — Dapper read of Finance.AccountingPeriod for cross-module consumers
    /// (backdate enforcement, year-end close, posting gate). Joins AppData.FinancialYear to
    /// surface the FY name (FinYearName) alongside the period row.
    /// </summary>
    internal sealed class AccountingPeriodLookupRepository : IAccountingPeriodLookup
    {
        private readonly IDbConnection _dbConnection;

        public AccountingPeriodLookupRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        private const string SelectClause = @"
            SELECT
                ap.Id, ap.FinancialYearId,
                fy.FinYearName AS FinancialYearName,
                ap.CompanyId,
                ap.PeriodNo, ap.PeriodName,
                ap.StartDate, ap.EndDate,
                ap.StatusId, fps.Code AS StatusCode,
                ap.IsAdjustmentPeriod
            FROM Finance.AccountingPeriod ap
            LEFT JOIN AppData.FinancialYear fy ON ap.FinancialYearId = fy.Id AND fy.IsDeleted = 0
            LEFT JOIN Finance.MiscMaster fps ON ap.StatusId = fps.Id AND fps.IsDeleted = 0";

        public async Task<IReadOnlyList<AccountingPeriodLookupDto>> GetAllPeriodsForCompanyAsync(int companyId, CancellationToken ct = default)
        {
            var sql = $@"{SelectClause}
                WHERE ap.CompanyId = @CompanyId AND ap.IsDeleted = 0
                ORDER BY ap.FinancialYearId DESC, ap.PeriodNo ASC";

            var result = await _dbConnection.QueryAsync<AccountingPeriodLookupDto>(
                new CommandDefinition(sql, new { CompanyId = companyId }, cancellationToken: ct));
            return result.ToList();
        }

        public async Task<AccountingPeriodLookupDto?> GetPeriodForDateAsync(int companyId, DateOnly date, CancellationToken ct = default)
        {
            // Adjustment period (PeriodNo = 13) is excluded — it shares date range with another period
            // and would double-match. Regular period selection is on date range BETWEEN.
            var sql = $@"{SelectClause}
                WHERE ap.CompanyId = @CompanyId
                  AND ap.IsDeleted = 0
                  AND ap.IsAdjustmentPeriod = 0
                  AND @Date BETWEEN ap.StartDate AND ap.EndDate";

            return await _dbConnection.QueryFirstOrDefaultAsync<AccountingPeriodLookupDto>(
                new CommandDefinition(sql, new { CompanyId = companyId, Date = date }, cancellationToken: ct));
        }

        public async Task<AccountingPeriodLookupDto?> GetByIdAsync(int periodId, int companyId, CancellationToken ct = default)
        {
            var sql = $@"{SelectClause}
                WHERE ap.Id = @PeriodId AND ap.CompanyId = @CompanyId AND ap.IsDeleted = 0";

            return await _dbConnection.QueryFirstOrDefaultAsync<AccountingPeriodLookupDto>(
                new CommandDefinition(sql, new { PeriodId = periodId, CompanyId = companyId }, cancellationToken: ct));
        }
    }
}
