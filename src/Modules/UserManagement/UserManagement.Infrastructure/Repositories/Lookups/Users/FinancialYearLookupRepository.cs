using System.Data;
using Dapper;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;

namespace UserManagement.Infrastructure.Repositories.Lookups.Users
{
    internal class FinancialYearLookupRepository : IFinancialYearLookup
    {
        private readonly IDbConnection _dbConnection;

        public FinancialYearLookupRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<List<FinancialYearLookupDto>> GetAllFinancialYearAsync()
        {
            const string sql = @"
                SELECT
                    Id            AS FinancialYearId,
                    FinYearName   AS FinancialYearName,
                    StartYear,
                    StartDate,
                    EndDate,
                    IsActive
                FROM [AppData].[FinancialYear]
                WHERE IsDeleted = 0
                ORDER BY StartDate DESC;
            ";

            var result = await _dbConnection.QueryAsync<FinancialYearLookupDto>(sql);
            return result.ToList();
        }

        public async Task<FinancialYearLookupDto?> GetByIdAsync(int financialYearId, CancellationToken ct = default)
        {
            const string sql = @"
                SELECT TOP 1
                    Id            AS FinancialYearId,
                    FinYearName   AS FinancialYearName,
                    StartYear,
                    StartDate,
                    EndDate,
                    IsActive
                FROM [AppData].[FinancialYear]
                WHERE IsDeleted = 0
                  AND Id = @FinancialYearId;
            ";

            return await _dbConnection.QueryFirstOrDefaultAsync<FinancialYearLookupDto>(
                new CommandDefinition(
                    sql,
                    new { FinancialYearId = financialYearId },
                    cancellationToken: ct));
        }

        public async Task<IReadOnlyList<FinancialYearLookupDto>> GetByIdsAsync(IEnumerable<int> financialYearIds, CancellationToken ct = default)
        {
            var ids = financialYearIds?
                .Where(id => id > 0)
                .Distinct()
                .ToArray() ?? Array.Empty<int>();

            if (ids.Length == 0)
                return Array.Empty<FinancialYearLookupDto>();

            const string sql = @"
                SELECT
                    Id            AS FinancialYearId,
                    FinYearName   AS FinancialYearName,
                    StartYear,
                    StartDate,
                    EndDate,
                    IsActive
                FROM [AppData].[FinancialYear]
                WHERE IsDeleted = 0
                  AND Id IN @FinancialYearIds;
            ";

            var result = await _dbConnection.QueryAsync<FinancialYearLookupDto>(
                new CommandDefinition(
                    sql,
                    new { FinancialYearIds = ids },
                    cancellationToken: ct));

            return result.ToList();
        }
    }
}
