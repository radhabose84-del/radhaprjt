using System.Data;
using Dapper;
using FinanceManagement.Application.Common.Interfaces.ICoaRead;
using FinanceManagement.Application.CoaRead.Dto;

namespace FinanceManagement.Infrastructure.Repositories.CoaRead
{
    // US-GL02-16 COA Read API — fast, downstream-facing reads. Get-by-code uses the unique
    // (CompanyId, AccountCode) index for the <100ms SLA (AC1).
    public class CoaReadQueryRepository : ICoaReadQueryRepository
    {
        private readonly IDbConnection _dbConnection;

        public CoaReadQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        private const string ReadSelect = @"
            am.Id, am.CompanyId, am.AccountCode, am.AccountName,
            am.AccountTypeId, atype.AccountTypeName,
            am.AccountGroupId, ag.GroupCode AS AccountGroupCode, ag.GroupName AS AccountGroupName,
            am.CurrencyTypeId, cfc.CurrencyTypeCode,
            nb.Code AS NormalBalanceCode,
            am.IsCostCentreMandatory, am.IsActive";

        private const string ReadFrom = @"
            FROM Finance.GlAccountMaster am
            LEFT JOIN Finance.AccountTypeMaster atype ON atype.Id = am.AccountTypeId AND atype.IsDeleted = 0
            LEFT JOIN Finance.AccountGroup ag ON ag.Id = am.AccountGroupId AND ag.IsDeleted = 0
            LEFT JOIN Finance.CurrencyForexConfig cfc ON cfc.Id = am.CurrencyTypeId AND cfc.IsDeleted = 0
            LEFT JOIN Finance.MiscMaster nb ON nb.Id = am.NormalBalanceId AND nb.IsDeleted = 0";

        public async Task<CoaAccountReadDto?> GetByCodeAsync(int companyId, string accountCode, CancellationToken ct)
        {
            var sql = $@"
                SELECT {ReadSelect}
                {ReadFrom}
                WHERE am.IsDeleted = 0 AND am.CompanyId = @CompanyId AND am.AccountCode = @AccountCode";

            return await _dbConnection.QueryFirstOrDefaultAsync<CoaAccountReadDto>(
                new CommandDefinition(sql, new { CompanyId = companyId, AccountCode = accountCode.Trim() }, cancellationToken: ct));
        }

        public async Task<List<CoaAccountReadDto>> SearchByTypeGroupAsync(
            int companyId, int? accountTypeId, int? accountGroupId, bool activeOnly, CancellationToken ct)
        {
            var sql = $@"
                SELECT {ReadSelect}
                {ReadFrom}
                WHERE am.IsDeleted = 0 AND am.CompanyId = @CompanyId
                  AND (@ActiveOnly = 0 OR am.IsActive = 1)
                  AND (@AccountTypeId IS NULL OR am.AccountTypeId = @AccountTypeId)
                  AND (@AccountGroupId IS NULL OR am.AccountGroupId = @AccountGroupId)
                ORDER BY am.AccountCode ASC";

            var rows = await _dbConnection.QueryAsync<CoaAccountReadDto>(new CommandDefinition(sql, new
            {
                CompanyId = companyId,
                AccountTypeId = accountTypeId,
                AccountGroupId = accountGroupId,
                ActiveOnly = activeOnly ? 1 : 0
            }, cancellationToken: ct));
            return rows.ToList();
        }

        private const string PostingInfoSelect = @"
            am.Id, am.AccountCode, am.AccountName, am.IsActive,
            am.CurrencyTypeId, cfc.CurrencyTypeCode, am.IsCostCentreMandatory
            FROM Finance.GlAccountMaster am
            LEFT JOIN Finance.CurrencyForexConfig cfc ON cfc.Id = am.CurrencyTypeId AND cfc.IsDeleted = 0";

        public async Task<AccountPostingInfo?> GetPostingInfoByCodeAsync(int companyId, string accountCode, CancellationToken ct)
        {
            var sql = $@"
                SELECT {PostingInfoSelect}
                WHERE am.IsDeleted = 0 AND am.CompanyId = @CompanyId AND am.AccountCode = @AccountCode";

            return await _dbConnection.QueryFirstOrDefaultAsync<AccountPostingInfo>(
                new CommandDefinition(sql, new { CompanyId = companyId, AccountCode = accountCode.Trim() }, cancellationToken: ct));
        }

        public async Task<AccountPostingInfo?> GetPostingInfoByIdAsync(int companyId, int id, CancellationToken ct)
        {
            var sql = $@"
                SELECT {PostingInfoSelect}
                WHERE am.IsDeleted = 0 AND am.CompanyId = @CompanyId AND am.Id = @Id";

            return await _dbConnection.QueryFirstOrDefaultAsync<AccountPostingInfo>(
                new CommandDefinition(sql, new { CompanyId = companyId, Id = id }, cancellationToken: ct));
        }
    }
}
