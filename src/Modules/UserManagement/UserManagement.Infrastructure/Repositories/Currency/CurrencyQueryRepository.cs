using System.Data;
using Dapper;
using UserManagement.Application.Common.Interfaces.ICurrency;

namespace UserManagement.Infrastructure.Repositories.Currency
{
    public class CurrencyQueryRepository : ICurrencyQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        public CurrencyQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<List<UserManagement.Domain.Entities.Currency>> GetByCurrencyNameAsync(string searchPattern)
        {
            searchPattern = searchPattern ?? string.Empty; // Prevent null issues

            const string query = @"
             SELECT Id, Code 
            FROM AppData.Currency
            WHERE IsDeleted = 0 
            AND Name LIKE @SearchPattern";
            var parameters = new
            {
                SearchPattern = $"%{searchPattern}%"
            };

            var currenciesGroups = await _dbConnection.QueryAsync<UserManagement.Domain.Entities.Currency>(query, parameters);
            return currenciesGroups.ToList();
        }

        public async Task<(List<UserManagement.Domain.Entities.Currency>, int)> GetAllCurrencyAsync(int PageNumber, int PageSize, string? SearchTerm)
        {
            var query = $$"""
             DECLARE @TotalCount INT;
             SELECT @TotalCount = COUNT(*) 
               FROM AppData.Currency
              WHERE IsDeleted = 0
            {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (Name LIKE @Search OR Code LIKE @Search)")}};

                SELECT 
                Id, 
                Code,
                Name,
                IsActive,
                CreatedBy,
                CreatedAt,
                CreatedByName,
                CreatedIP,
                ModifiedBy,
                ModifiedAt,
                ModifiedByName,
                ModifiedIP
               
            FROM AppData.Currency 
            WHERE 
            IsDeleted = 0
                {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (Name LIKE @Search OR Code LIKE @Search )")}}
                ORDER BY Id desc
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;
            """;


            var parameters = new
            {
                Search = $"%{SearchTerm}%",
                Offset = (PageNumber - 1) * PageSize,
                PageSize
            };

            var currencygroup = await _dbConnection.QueryMultipleAsync(query, parameters);
            var currenciesgrouplist = (await currencygroup.ReadAsync<UserManagement.Domain.Entities.Currency>()).ToList();
            int totalCount = (await currencygroup.ReadFirstAsync<int>());
            return (currenciesgrouplist, totalCount);
        }

        public async Task<UserManagement.Domain.Entities.Currency?> GetByIdAsync(int id)
        {
            const string query = @"
                    SELECT * 
                    FROM AppData.Currency 
                    WHERE Id = @Id AND IsDeleted = 0";
            var currencyGroup = await _dbConnection.QueryFirstOrDefaultAsync<UserManagement.Domain.Entities.Currency>(query, new { id });
            return currencyGroup;
        }
        public async Task<List<UserManagement.Domain.Entities.Currency>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken ct = default)
        {
            var idList = ids?.Distinct().ToArray() ?? Array.Empty<int>();
            if (idList.Length == 0) return new List<UserManagement.Domain.Entities.Currency>();

            const string sql = @"SELECT Id, Code, Name
                                FROM AppData.Currency
                                WHERE IsDeleted = 0 AND Id IN @ids";
            var list = await _dbConnection.QueryAsync<UserManagement.Domain.Entities.Currency>(sql, new { ids = idList });
            return list.ToList();
        }
    }
}