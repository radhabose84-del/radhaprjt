using System.Data;
using Contracts.Interfaces.Lookups.Users;
using Dapper;
using FinanceManagement.Application.AccountTypeMaster.Dto;
using FinanceManagement.Application.Common.Interfaces.IAccountTypeMaster;

namespace FinanceManagement.Infrastructure.Repositories.AccountTypeMaster
{
    public class AccountTypeMasterQueryRepository : IAccountTypeMasterQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly ICompanyLookup _companyLookup;

        public AccountTypeMasterQueryRepository(IDbConnection dbConnection, ICompanyLookup companyLookup)
        {
            _dbConnection = dbConnection;
            _companyLookup = companyLookup;
        }

        public async Task<(List<AccountTypeMasterDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm, int? companyId = null)
        {
            var whereClause = "atm.IsDeleted = 0";
            if (!string.IsNullOrWhiteSpace(searchTerm))
                whereClause += " AND (atm.AccountTypeName LIKE @Search OR atm.StartCode LIKE @Search)";
            if (companyId.HasValue && companyId.Value > 0)
                whereClause += " AND atm.CompanyId = @CompanyId";

            var query = $@"
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*)
                FROM Finance.AccountTypeMaster atm
                WHERE {whereClause};

                SELECT atm.Id, atm.CompanyId, atm.AccountTypeName, atm.StartCode, atm.AccountCodeLength, atm.SortOrder,
                    atm.IsActive, atm.IsDeleted,
                    atm.CreatedBy, atm.CreatedDate, atm.CreatedByName, atm.CreatedIP,
                    atm.ModifiedBy, atm.ModifiedDate, atm.ModifiedByName, atm.ModifiedIP
                FROM Finance.AccountTypeMaster atm
                WHERE {whereClause}
                ORDER BY atm.CompanyId ASC, atm.SortOrder ASC, atm.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;
            ";

            var parameters = new { Search = $"%{searchTerm}%", CompanyId = companyId, Offset = (pageNumber - 1) * pageSize, PageSize = pageSize };
            var result = await _dbConnection.QueryMultipleAsync(query, parameters);
            var list = (await result.ReadAsync<AccountTypeMasterDto>()).ToList();
            var totalCount = await result.ReadFirstAsync<int>();

            if (list.Count > 0)
            {
                var companies = await _companyLookup.GetAllCompanyAsync();
                var companyDict = companies.ToDictionary(c => c.CompanyId, c => c.CompanyName);
                foreach (var item in list)
                {
                    item.CompanyName = companyDict.TryGetValue(item.CompanyId, out var name) ? name : null;
                }
            }

            return (list, totalCount);
        }

        public async Task<AccountTypeMasterDto?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT atm.Id, atm.CompanyId, atm.AccountTypeName, atm.StartCode, atm.AccountCodeLength, atm.SortOrder,
                    atm.IsActive, atm.IsDeleted,
                    atm.CreatedBy, atm.CreatedDate, atm.CreatedByName, atm.CreatedIP,
                    atm.ModifiedBy, atm.ModifiedDate, atm.ModifiedByName, atm.ModifiedIP
                FROM Finance.AccountTypeMaster atm
                WHERE atm.Id = @Id AND atm.IsDeleted = 0";

            var dto = await _dbConnection.QueryFirstOrDefaultAsync<AccountTypeMasterDto>(sql, new { Id = id });

            if (dto != null)
            {
                var companies = await _companyLookup.GetAllCompanyAsync();
                var company = companies.FirstOrDefault(c => c.CompanyId == dto.CompanyId);
                dto.CompanyName = company?.CompanyName;
            }

            return dto;
        }

        public async Task<IReadOnlyList<AccountTypeMasterLookupDto>> AutocompleteAsync(string term, int? companyId, CancellationToken ct)
        {
            var whereClause = "atm.IsDeleted = 0 AND atm.IsActive = 1";
            if (!string.IsNullOrWhiteSpace(term))
                whereClause += " AND (atm.AccountTypeName LIKE @Term OR atm.StartCode LIKE @Term)";
            if (companyId.HasValue && companyId.Value > 0)
                whereClause += " AND atm.CompanyId = @CompanyId";

            var sql = $@"
                SELECT atm.Id, atm.CompanyId, atm.AccountTypeName, atm.StartCode, atm.AccountCodeLength, atm.SortOrder
                FROM Finance.AccountTypeMaster atm
                WHERE {whereClause}
                ORDER BY atm.CompanyId ASC, atm.SortOrder ASC";

            var result = await _dbConnection.QueryAsync<AccountTypeMasterLookupDto>(
                new CommandDefinition(sql, new { Term = $"%{term}%", CompanyId = companyId }, cancellationToken: ct));
            return result.ToList();
        }

        public async Task<bool> AlreadyExistsByNameAsync(string accountTypeName, int companyId, int? id = null)
        {
            var sql = @"
                SELECT COUNT(1)
                FROM Finance.AccountTypeMaster
                WHERE AccountTypeName = @AccountTypeName AND CompanyId = @CompanyId
                AND IsDeleted = 0";

            if (id.HasValue && id.Value > 0)
                sql += " AND Id != @Id";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { AccountTypeName = accountTypeName.Trim(), CompanyId = companyId, Id = id });
            return count > 0;
        }

        public async Task<bool> AlreadyExistsByStartCodeAsync(string startCode, int companyId, int? id = null)
        {
            var sql = @"
                SELECT COUNT(1)
                FROM Finance.AccountTypeMaster
                WHERE StartCode = @StartCode AND CompanyId = @CompanyId
                AND IsDeleted = 0";

            if (id.HasValue && id.Value > 0)
                sql += " AND Id != @Id";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { StartCode = startCode.Trim(), CompanyId = companyId, Id = id });
            return count > 0;
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Finance.AccountTypeMaster
                WHERE Id = @Id AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }

        // No Finance.AccountMaster dependent exists yet — extend with EXISTS clauses when added.
        public Task<bool> SoftDeleteValidationAsync(int id) => Task.FromResult(false);

        public Task<bool> IsAccountTypeLinkedAsync(int id) => Task.FromResult(false);
    }
}
