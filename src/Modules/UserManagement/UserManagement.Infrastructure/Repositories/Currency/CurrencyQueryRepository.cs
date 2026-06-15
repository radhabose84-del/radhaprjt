using System.Data;
using Dapper;
using UserManagement.Application.Common.Interfaces.ICurrency;
using Contracts.Interfaces.Validations.SalesManagement;
using Contracts.Interfaces.Validations.PurchaseManagement;
using Contracts.Interfaces.Validations.BudgetManagement;
using Contracts.Interfaces.Validations.ProjectManagement;

namespace UserManagement.Infrastructure.Repositories.Currency
{
    public class CurrencyQueryRepository : ICurrencyQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly ISalesCurrencyValidation _salesCurrencyValidation;
        private readonly IPurchaseCurrencyValidation _purchaseCurrencyValidation;
        private readonly IBudgetCurrencyValidation _budgetCurrencyValidation;
        private readonly IProjectCurrencyValidation _projectCurrencyValidation;

        public CurrencyQueryRepository(
            IDbConnection dbConnection,
            ISalesCurrencyValidation salesCurrencyValidation,
            IPurchaseCurrencyValidation purchaseCurrencyValidation,
            IBudgetCurrencyValidation budgetCurrencyValidation,
            IProjectCurrencyValidation projectCurrencyValidation)
        {
            _dbConnection = dbConnection;
            _salesCurrencyValidation = salesCurrencyValidation;
            _purchaseCurrencyValidation = purchaseCurrencyValidation;
            _budgetCurrencyValidation = budgetCurrencyValidation;
            _projectCurrencyValidation = projectCurrencyValidation;
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

        /// <inheritdoc />
        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = "SELECT COUNT(1) FROM AppData.Currency WHERE Id = @Id AND IsDeleted = 0";
            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count == 0;
        }

        /// <inheritdoc />
        public async Task<bool> SoftDeleteValidationAsync(int id)
        {
            // Same-module: check if any non-deleted CompanySetting references this currency
            const string sql = @"
                SELECT CASE WHEN
                    EXISTS (SELECT 1 FROM [AppData].[CompanySetting] WHERE CurrencyId = @Id AND IsDeleted = 0)
                THEN 1 ELSE 0 END";

            var inModule = await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = id });
            if (inModule) return true;

            // Cross-module checks via validation interfaces
            if (await _salesCurrencyValidation.HasLinkedCurrencyAsync(id)) return true;
            if (await _purchaseCurrencyValidation.HasLinkedCurrencyAsync(id)) return true;
            if (await _budgetCurrencyValidation.HasLinkedCurrencyAsync(id)) return true;
            if (await _projectCurrencyValidation.HasLinkedCurrencyAsync(id)) return true;

            return false;
        }

        /// <inheritdoc />
        public Task<bool> IsCurrencyLinkedAsync(int id)
        {
            // Currency is a universal master — skip inactivate guard (too foundational).
            // Only delete guard is enforced via SoftDeleteValidationAsync.
            return Task.FromResult(false);
        }
    }
}