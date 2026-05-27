using System.Data;
using Contracts.Dtos.Lookups.Purchase;
using Dapper;
using PurchaseManagement.Application.Common.Interfaces.IVendorEvaluationCriteria;
using PurchaseManagement.Application.VendorEvaluationCriteria.Dto;

namespace PurchaseManagement.Infrastructure.Repositories.VendorEvaluationCriteria
{
    public class VendorEvaluationCriteriaQueryRepository : IVendorEvaluationCriteriaQueryRepository
    {
        private readonly IDbConnection _dbConnection;

        public VendorEvaluationCriteriaQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<(List<VendorEvaluationCriteriaDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            var offset = (pageNumber - 1) * pageSize;

            const string countSql = @"
                SELECT COUNT(*)
                FROM Purchase.VendorEvaluationCriteria vec
                WHERE vec.IsDeleted = 0
                  AND (@SearchTerm IS NULL OR vec.CriteriaCode LIKE '%' + @SearchTerm + '%'
                       OR vec.CriteriaName LIKE '%' + @SearchTerm + '%')";

            const string dataSql = @"
                SELECT vec.Id, vec.CriteriaCode, vec.CriteriaName, vec.Description,
                       vec.WeightagePercent, vec.ScoringMethodId, vec.MinimumScore,
                       vec.RatingImpactId, vec.SortOrder, vec.IsActive, vec.IsDeleted,
                       vec.CreatedBy, vec.CreatedDate, vec.CreatedByName, vec.CreatedIP,
                       vec.ModifiedBy, vec.ModifiedDate, vec.ModifiedByName, vec.ModifiedIP,
                       sm.Description AS ScoringMethodName,
                       ri.Description AS RatingImpactName
                FROM Purchase.VendorEvaluationCriteria vec
                LEFT JOIN Purchase.MiscMaster sm ON vec.ScoringMethodId = sm.Id AND sm.IsDeleted = 0
                LEFT JOIN Purchase.MiscMaster ri ON vec.RatingImpactId = ri.Id AND ri.IsDeleted = 0
                WHERE vec.IsDeleted = 0
                  AND (@SearchTerm IS NULL OR vec.CriteriaCode LIKE '%' + @SearchTerm + '%'
                       OR vec.CriteriaName LIKE '%' + @SearchTerm + '%')
                ORDER BY vec.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var totalCount = await _dbConnection.ExecuteScalarAsync<int>(countSql, new { SearchTerm = searchTerm });
            var data = (await _dbConnection.QueryAsync<VendorEvaluationCriteriaDto>(dataSql, new { SearchTerm = searchTerm, Offset = offset, PageSize = pageSize })).ToList();

            return (data, totalCount);
        }

        public async Task<VendorEvaluationCriteriaDto?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT vec.Id, vec.CriteriaCode, vec.CriteriaName, vec.Description,
                       vec.WeightagePercent, vec.ScoringMethodId, vec.MinimumScore,
                       vec.RatingImpactId, vec.SortOrder, vec.IsActive, vec.IsDeleted,
                       vec.CreatedBy, vec.CreatedDate, vec.CreatedByName, vec.CreatedIP,
                       vec.ModifiedBy, vec.ModifiedDate, vec.ModifiedByName, vec.ModifiedIP,
                       sm.Description AS ScoringMethodName,
                       ri.Description AS RatingImpactName
                FROM Purchase.VendorEvaluationCriteria vec
                LEFT JOIN Purchase.MiscMaster sm ON vec.ScoringMethodId = sm.Id AND sm.IsDeleted = 0
                LEFT JOIN Purchase.MiscMaster ri ON vec.RatingImpactId = ri.Id AND ri.IsDeleted = 0
                WHERE vec.Id = @Id AND vec.IsDeleted = 0";

            return await _dbConnection.QueryFirstOrDefaultAsync<VendorEvaluationCriteriaDto>(sql, new { Id = id });
        }

        public async Task<IReadOnlyList<VendorEvaluationCriteriaLookupDto>> AutocompleteAsync(string term, CancellationToken ct)
        {
            const string sql = @"
                SELECT Id, CriteriaCode, CriteriaName
                FROM Purchase.VendorEvaluationCriteria
                WHERE IsActive = 1 AND IsDeleted = 0
                  AND (@Term IS NULL OR @Term = '' OR CriteriaCode LIKE '%' + @Term + '%'
                       OR CriteriaName LIKE '%' + @Term + '%')
                ORDER BY CriteriaName ASC";

            var result = await _dbConnection.QueryAsync<VendorEvaluationCriteriaLookupDto>(
                new CommandDefinition(sql, new { Term = term }, cancellationToken: ct));
            return result.ToList();
        }

        public async Task<bool> AlreadyExistsAsync(string criteriaCode, int? id = null)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM Purchase.VendorEvaluationCriteria
                    WHERE CriteriaCode = @CriteriaCode AND IsDeleted = 0
                      AND (@Id IS NULL OR Id != @Id)
                ) THEN 1 ELSE 0 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { CriteriaCode = criteriaCode, Id = id });
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = @"
                SELECT CASE WHEN NOT EXISTS (
                    SELECT 1 FROM Purchase.VendorEvaluationCriteria
                    WHERE Id = @Id AND IsDeleted = 0
                ) THEN 1 ELSE 0 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = id });
        }

        public async Task<bool> ScoringMethodExistsAsync(int scoringMethodId)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM Purchase.MiscMaster
                    WHERE Id = @Id AND IsDeleted = 0 AND IsActive = 1
                ) THEN 1 ELSE 0 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = scoringMethodId });
        }

        public async Task<bool> RatingImpactExistsAsync(int ratingImpactId)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM Purchase.MiscMaster
                    WHERE Id = @Id AND IsDeleted = 0 AND IsActive = 1
                ) THEN 1 ELSE 0 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = ratingImpactId });
        }
    }
}
