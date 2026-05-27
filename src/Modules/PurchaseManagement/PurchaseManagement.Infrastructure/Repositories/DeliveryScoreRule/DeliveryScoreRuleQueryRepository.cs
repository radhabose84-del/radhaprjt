using System.Data;
using Contracts.Dtos.Lookups.Purchase;
using Dapper;
using PurchaseManagement.Application.Common.Interfaces.IDeliveryScoreRule;
using PurchaseManagement.Application.DeliveryScoreRule.Dto;

namespace PurchaseManagement.Infrastructure.Repositories.DeliveryScoreRule
{
    public class DeliveryScoreRuleQueryRepository : IDeliveryScoreRuleQueryRepository
    {
        private readonly IDbConnection _dbConnection;

        public DeliveryScoreRuleQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<(List<DeliveryScoreRuleDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            var offset = (pageNumber - 1) * pageSize;

            const string countSql = @"
                SELECT COUNT(*)
                FROM Purchase.DeliveryScoreRule dsr
                WHERE dsr.IsDeleted = 0
                  AND (@SearchTerm IS NULL OR dsr.RuleCode LIKE '%' + @SearchTerm + '%'
                       OR dsr.Description LIKE '%' + @SearchTerm + '%')";

            const string dataSql = @"
                SELECT dsr.Id, dsr.RuleCode, dsr.Description, dsr.DelayDaysFrom, dsr.DelayDaysTo,
                       dsr.Score, dsr.SortOrder, dsr.IsActive, dsr.IsDeleted,
                       dsr.CreatedBy, dsr.CreatedDate, dsr.CreatedByName, dsr.CreatedIP,
                       dsr.ModifiedBy, dsr.ModifiedDate, dsr.ModifiedByName, dsr.ModifiedIP
                FROM Purchase.DeliveryScoreRule dsr
                WHERE dsr.IsDeleted = 0
                  AND (@SearchTerm IS NULL OR dsr.RuleCode LIKE '%' + @SearchTerm + '%'
                       OR dsr.Description LIKE '%' + @SearchTerm + '%')
                ORDER BY dsr.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var totalCount = await _dbConnection.ExecuteScalarAsync<int>(countSql, new { SearchTerm = searchTerm });
            var data = (await _dbConnection.QueryAsync<DeliveryScoreRuleDto>(dataSql, new { SearchTerm = searchTerm, Offset = offset, PageSize = pageSize })).ToList();

            return (data, totalCount);
        }

        public async Task<DeliveryScoreRuleDto?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT Id, RuleCode, Description, DelayDaysFrom, DelayDaysTo,
                       Score, SortOrder, IsActive, IsDeleted,
                       CreatedBy, CreatedDate, CreatedByName, CreatedIP,
                       ModifiedBy, ModifiedDate, ModifiedByName, ModifiedIP
                FROM Purchase.DeliveryScoreRule
                WHERE Id = @Id AND IsDeleted = 0";

            return await _dbConnection.QueryFirstOrDefaultAsync<DeliveryScoreRuleDto>(sql, new { Id = id });
        }

        public async Task<IReadOnlyList<DeliveryScoreRuleLookupDto>> AutocompleteAsync(string term, CancellationToken ct)
        {
            const string sql = @"
                SELECT Id, RuleCode, Description
                FROM Purchase.DeliveryScoreRule
                WHERE IsActive = 1 AND IsDeleted = 0
                  AND (@Term IS NULL OR @Term = '' OR RuleCode LIKE '%' + @Term + '%'
                       OR Description LIKE '%' + @Term + '%')
                ORDER BY Description ASC";

            var result = await _dbConnection.QueryAsync<DeliveryScoreRuleLookupDto>(
                new CommandDefinition(sql, new { Term = term }, cancellationToken: ct));
            return result.ToList();
        }

        public async Task<bool> AlreadyExistsAsync(string ruleCode, int? id = null)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM Purchase.DeliveryScoreRule
                    WHERE RuleCode = @RuleCode AND IsDeleted = 0
                      AND (@Id IS NULL OR Id != @Id)
                ) THEN 1 ELSE 0 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { RuleCode = ruleCode, Id = id });
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = @"
                SELECT CASE WHEN NOT EXISTS (
                    SELECT 1 FROM Purchase.DeliveryScoreRule
                    WHERE Id = @Id AND IsDeleted = 0
                ) THEN 1 ELSE 0 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = id });
        }
    }
}
