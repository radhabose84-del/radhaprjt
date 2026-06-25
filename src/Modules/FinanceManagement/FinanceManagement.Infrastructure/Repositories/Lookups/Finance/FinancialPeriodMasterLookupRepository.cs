using System.Data;
using Contracts.Dtos.Lookups.Finance;
using Contracts.Interfaces.Lookups.Finance;
using Dapper;

namespace FinanceManagement.Infrastructure.Repositories.Lookups.Finance
{
    internal sealed class FinancialPeriodMasterLookupRepository : IFinancialPeriodMasterLookup
    {
        private readonly IDbConnection _dbConnection;

        public FinancialPeriodMasterLookupRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        private const string BaseSelect = @"
            fp.Id, fp.FinancialYearId, fy.FinancialYearCode, fp.CompanyId,
            fp.PeriodNumber, fp.PeriodName, fp.StartDate, fp.EndDate,
            fp.StatusId, fps.Code AS StatusCode, fp.IsAdjustmentPeriod
            FROM Finance.FinancialPeriodMaster fp
            LEFT JOIN Finance.FinancialYearMaster fy ON fp.FinancialYearId = fy.Id AND fy.IsDeleted = 0
            LEFT JOIN Finance.MiscMaster fps ON fp.StatusId = fps.Id AND fps.IsDeleted = 0
        ";

        public async Task<IReadOnlyList<FinancialPeriodMasterLookupDto>> GetAllPeriodsForCompanyAsync(int companyId, CancellationToken ct = default)
        {
            var sql = $@"
                SELECT {BaseSelect}
                WHERE fp.CompanyId = @CompanyId AND fp.IsActive = 1 AND fp.IsDeleted = 0
                ORDER BY fp.StartDate ASC, fp.PeriodNumber ASC";

            var result = await _dbConnection.QueryAsync<FinancialPeriodMasterLookupDto>(
                new CommandDefinition(sql, new { CompanyId = companyId }, cancellationToken: ct));
            return result.ToList();
        }

        public async Task<FinancialPeriodMasterLookupDto?> GetPeriodForDateAsync(int companyId, DateOnly date, CancellationToken ct = default)
        {
            var sql = $@"
                SELECT TOP 1 {BaseSelect}
                WHERE fp.CompanyId = @CompanyId
                  AND fp.IsActive = 1 AND fp.IsDeleted = 0
                  AND fp.IsAdjustmentPeriod = 0
                  AND @Date BETWEEN fp.StartDate AND fp.EndDate
                ORDER BY fp.PeriodNumber ASC";

            return await _dbConnection.QueryFirstOrDefaultAsync<FinancialPeriodMasterLookupDto>(
                new CommandDefinition(sql, new { CompanyId = companyId, Date = date }, cancellationToken: ct));
        }

        public async Task<FinancialPeriodMasterLookupDto?> GetByIdAsync(int periodId, int companyId, CancellationToken ct = default)
        {
            var sql = $@"
                SELECT {BaseSelect}
                WHERE fp.Id = @Id AND fp.CompanyId = @CompanyId
                  AND fp.IsActive = 1 AND fp.IsDeleted = 0";

            return await _dbConnection.QueryFirstOrDefaultAsync<FinancialPeriodMasterLookupDto>(
                new CommandDefinition(sql, new { Id = periodId, CompanyId = companyId }, cancellationToken: ct));
        }
    }
}
