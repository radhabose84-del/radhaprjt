using System.Data;
using Contracts.Dtos.Lookups.Finance;
using Contracts.Interfaces.Lookups.Finance;
using Dapper;

namespace FinanceManagement.Infrastructure.Repositories.Lookups.Finance
{
    internal sealed class GlAccountMasterLookupRepository : IGlAccountMasterLookup
    {
        private readonly IDbConnection _dbConnection;

        public GlAccountMasterLookupRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        private const string BaseSelect = @"
            am.Id, am.CompanyId, am.AccountTypeId,
            am.AccountCode, am.AccountName,
            nb.Code AS NormalBalanceCode
            FROM Finance.GlAccountMaster am
            LEFT JOIN Finance.MiscMaster nb ON am.NormalBalanceId = nb.Id AND nb.IsDeleted = 0
        ";

        public async Task<IReadOnlyList<GlAccountMasterLookupDto>> GetAllForCompanyAsync(int companyId, CancellationToken ct = default)
        {
            var sql = $@"
                SELECT {BaseSelect}
                WHERE am.CompanyId = @CompanyId AND am.IsActive = 1 AND am.IsDeleted = 0
                ORDER BY am.AccountCode ASC";

            var result = await _dbConnection.QueryAsync<GlAccountMasterLookupDto>(
                new CommandDefinition(sql, new { CompanyId = companyId }, cancellationToken: ct));
            return result.ToList();
        }

        public async Task<GlAccountMasterLookupDto?> GetByIdForCompanyAsync(int id, int companyId, CancellationToken ct = default)
        {
            var sql = $@"
                SELECT {BaseSelect}
                WHERE am.Id = @Id AND am.CompanyId = @CompanyId AND am.IsActive = 1 AND am.IsDeleted = 0";

            return await _dbConnection.QueryFirstOrDefaultAsync<GlAccountMasterLookupDto>(
                new CommandDefinition(sql, new { Id = id, CompanyId = companyId }, cancellationToken: ct));
        }

        public async Task<GlAccountMasterLookupDto?> GetByCodeForCompanyAsync(string accountCode, int companyId, CancellationToken ct = default)
        {
            var sql = $@"
                SELECT {BaseSelect}
                WHERE am.AccountCode = @AccountCode AND am.CompanyId = @CompanyId AND am.IsActive = 1 AND am.IsDeleted = 0";

            return await _dbConnection.QueryFirstOrDefaultAsync<GlAccountMasterLookupDto>(
                new CommandDefinition(sql, new { AccountCode = accountCode, CompanyId = companyId }, cancellationToken: ct));
        }
    }
}
