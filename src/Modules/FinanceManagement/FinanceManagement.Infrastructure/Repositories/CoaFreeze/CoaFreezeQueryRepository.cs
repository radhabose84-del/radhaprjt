using System.Data;
using Dapper;
using FinanceManagement.Application.Common.Interfaces.ICoaFreeze;

namespace FinanceManagement.Infrastructure.Repositories.CoaFreeze
{
    public class CoaFreezeQueryRepository : ICoaFreezeQueryRepository
    {
        private readonly IDbConnection _dbConnection;

        public CoaFreezeQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<CoaFreezeStateRow?> GetStateAsync(int companyId, CancellationToken ct)
        {
            const string sql = @"
                SELECT TOP 1 IsFrozen, FrozenByUserId, FrozenOn, UnfreezeWindowExpiry
                FROM Finance.CoaFreezeState
                WHERE CompanyId = @CompanyId AND IsDeleted = 0";

            return await _dbConnection.QueryFirstOrDefaultAsync<CoaFreezeStateRow>(
                new CommandDefinition(sql, new { CompanyId = companyId }, cancellationToken: ct));
        }

        public async Task<bool> AreTriggersActiveAsync(CancellationToken ct)
        {
            // Both enforcement triggers must exist AND be enabled → "DB Trigger: ACTIVE".
            const string sql = @"
                SELECT COUNT(1) FROM sys.triggers
                WHERE name IN ('trg_GlAccountMaster_CoaFreeze', 'trg_AccountGroup_CoaFreeze')
                  AND is_disabled = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(
                new CommandDefinition(sql, cancellationToken: ct));
            return count >= 2;
        }

        public async Task<(int TotalAccounts, int TotalAccountGroups)> GetCoaCountsAsync(int companyId, CancellationToken ct)
        {
            const string sql = @"
                SELECT
                    (SELECT COUNT(1) FROM Finance.GlAccountMaster WHERE CompanyId = @CompanyId AND IsDeleted = 0) AS TotalAccounts,
                    (SELECT COUNT(1) FROM Finance.AccountGroup    WHERE CompanyId = @CompanyId AND IsDeleted = 0) AS TotalAccountGroups";

            var row = await _dbConnection.QueryFirstAsync<CoaCountsRow>(
                new CommandDefinition(sql, new { CompanyId = companyId }, cancellationToken: ct));
            return (row.TotalAccounts, row.TotalAccountGroups);
        }

        private sealed class CoaCountsRow
        {
            public int TotalAccounts { get; set; }
            public int TotalAccountGroups { get; set; }
        }
    }
}
