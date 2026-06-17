using System.Data;
using Contracts.Interfaces.Lookups.Users;
using Dapper;
using FinanceManagement.Application.Common.Interfaces.ICurrencyForexConfig;
using FinanceManagement.Application.CurrencyForexConfig.Dto;

namespace FinanceManagement.Infrastructure.Repositories.CurrencyForexConfig
{
    public class CurrencyForexConfigQueryRepository : ICurrencyForexConfigQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly ICompanyLookup _companyLookup;

        public CurrencyForexConfigQueryRepository(IDbConnection dbConnection, ICompanyLookup companyLookup)
        {
            _dbConnection = dbConnection;
            _companyLookup = companyLookup;
        }

        private const string BaseSelect = @"
            cfc.Id, cfc.CompanyId,
            cfc.CurrencyTypeCode, cfc.CurrencyTypeName,
            cfc.IsActive, cfc.IsDeleted,
            cfc.CreatedBy, cfc.CreatedDate, cfc.CreatedByName, cfc.CreatedIP,
            cfc.ModifiedBy, cfc.ModifiedDate, cfc.ModifiedByName, cfc.ModifiedIP
        ";

        public async Task<(List<CurrencyForexConfigDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm, int companyId)
        {
            var whereClause = "cfc.IsDeleted = 0 AND cfc.CompanyId = @CompanyId";
            if (!string.IsNullOrWhiteSpace(searchTerm))
                whereClause += " AND (cfc.CurrencyTypeCode LIKE @Search OR cfc.CurrencyTypeName LIKE @Search)";

            var query = $@"
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*)
                FROM Finance.CurrencyForexConfig cfc
                WHERE {whereClause};

                SELECT {BaseSelect}
                FROM Finance.CurrencyForexConfig cfc
                WHERE {whereClause}
                ORDER BY cfc.CurrencyTypeCode ASC, cfc.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;
            ";

            var parameters = new
            {
                Search = $"%{searchTerm}%",
                CompanyId = companyId,
                Offset = (pageNumber - 1) * pageSize,
                PageSize = pageSize
            };

            var result = await _dbConnection.QueryMultipleAsync(query, parameters);
            var list = (await result.ReadAsync<CurrencyForexConfigDto>()).ToList();
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

        public async Task<CurrencyForexConfigDto?> GetByIdAsync(int id)
        {
            var sql = $@"
                SELECT {BaseSelect}
                FROM Finance.CurrencyForexConfig cfc
                WHERE cfc.Id = @Id AND cfc.IsDeleted = 0";

            var dto = await _dbConnection.QueryFirstOrDefaultAsync<CurrencyForexConfigDto>(sql, new { Id = id });

            if (dto != null)
            {
                var companies = await _companyLookup.GetAllCompanyAsync();
                var company = companies.FirstOrDefault(c => c.CompanyId == dto.CompanyId);
                dto.CompanyName = company?.CompanyName;
            }

            return dto;
        }

        public async Task<IReadOnlyList<CurrencyForexConfigLookupDto>> AutocompleteAsync(string term, int companyId, CancellationToken ct)
        {
            var whereClause = "cfc.IsDeleted = 0 AND cfc.IsActive = 1 AND cfc.CompanyId = @CompanyId";
            if (!string.IsNullOrWhiteSpace(term))
                whereClause += " AND (cfc.CurrencyTypeCode LIKE @Term OR cfc.CurrencyTypeName LIKE @Term)";

            var sql = $@"
                SELECT cfc.Id, cfc.CompanyId, cfc.CurrencyTypeCode, cfc.CurrencyTypeName
                FROM Finance.CurrencyForexConfig cfc
                WHERE {whereClause}
                ORDER BY cfc.CurrencyTypeCode ASC";

            var result = await _dbConnection.QueryAsync<CurrencyForexConfigLookupDto>(
                new CommandDefinition(sql, new { Term = $"%{term}%", CompanyId = companyId }, cancellationToken: ct));
            return result.ToList();
        }

        public async Task<bool> AlreadyExistsByCodeAsync(string currencyTypeCode, int companyId, int? id = null)
        {
            var sql = @"
                SELECT COUNT(1)
                FROM Finance.CurrencyForexConfig
                WHERE CurrencyTypeCode = @Code AND CompanyId = @CompanyId AND IsDeleted = 0";

            if (id.HasValue && id.Value > 0)
                sql += " AND Id != @Id";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Code = currencyTypeCode.Trim(), CompanyId = companyId, Id = id });
            return count > 0;
        }

        public async Task<bool> AlreadyExistsByNameAsync(string currencyTypeName, int companyId, int? id = null)
        {
            var sql = @"
                SELECT COUNT(1)
                FROM Finance.CurrencyForexConfig
                WHERE CurrencyTypeName = @Name AND CompanyId = @CompanyId AND IsDeleted = 0";

            if (id.HasValue && id.Value > 0)
                sql += " AND Id != @Id";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Name = currencyTypeName.Trim(), CompanyId = companyId, Id = id });
            return count > 0;
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM Finance.CurrencyForexConfig
                WHERE Id = @Id AND IsDeleted = 0";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }

        // Rule 25 — a GL account references this config via GlAccountMaster.CurrencyTypeId
        public async Task<bool> SoftDeleteValidationAsync(int id)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM Finance.GlAccountMaster
                    WHERE CurrencyTypeId = @Id AND IsDeleted = 0
                ) THEN 1 ELSE 0 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = id });
        }

        public async Task<bool> IsCurrencyForexConfigLinkedAsync(int id)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM Finance.GlAccountMaster
                    WHERE CurrencyTypeId = @Id AND IsDeleted = 0 AND IsActive = 1
                ) THEN 1 ELSE 0 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = id });
        }
    }
}
