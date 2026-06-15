using System.Data;
using Contracts.Dtos.Lookups.Finance;
using Contracts.Interfaces.Lookups.Finance;
using Dapper;

namespace FinanceManagement.Infrastructure.Repositories.Lookups.Finance
{
    internal sealed class AccountTypeMasterLookupRepository : IAccountTypeMasterLookup
    {
        private readonly IDbConnection _dbConnection;

        public AccountTypeMasterLookupRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<IReadOnlyList<AccountTypeMasterLookupDto>> GetAllForCompanyAsync(int companyId, CancellationToken ct = default)
        {
            const string sql = @"
                SELECT Id, CompanyId, AccountTypeName, StartCode, AccountCodeLength, SortOrder
                FROM Finance.AccountTypeMaster
                WHERE CompanyId = @CompanyId AND IsActive = 1 AND IsDeleted = 0
                ORDER BY SortOrder ASC";

            var result = await _dbConnection.QueryAsync<AccountTypeMasterLookupDto>(
                new CommandDefinition(sql, new { CompanyId = companyId }, cancellationToken: ct));
            return result.ToList();
        }

        public async Task<AccountTypeMasterLookupDto?> GetByIdForCompanyAsync(int id, int companyId, CancellationToken ct = default)
        {
            const string sql = @"
                SELECT Id, CompanyId, AccountTypeName, StartCode, AccountCodeLength, SortOrder
                FROM Finance.AccountTypeMaster
                WHERE Id = @Id AND CompanyId = @CompanyId AND IsActive = 1 AND IsDeleted = 0";

            return await _dbConnection.QueryFirstOrDefaultAsync<AccountTypeMasterLookupDto>(
                new CommandDefinition(sql, new { Id = id, CompanyId = companyId }, cancellationToken: ct));
        }
    }
}
