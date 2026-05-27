using System.Data;
using Contracts.Dtos.Lookups.Purchase;
using Dapper;
using PurchaseManagement.Application.Common.Interfaces.IVendorRatingGrade;
using PurchaseManagement.Application.VendorRatingGrade.Dto;

namespace PurchaseManagement.Infrastructure.Repositories.VendorRatingGrade
{
    public class VendorRatingGradeQueryRepository : IVendorRatingGradeQueryRepository
    {
        private readonly IDbConnection _dbConnection;

        public VendorRatingGradeQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<(List<VendorRatingGradeDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            var offset = (pageNumber - 1) * pageSize;

            const string countSql = @"
                SELECT COUNT(*)
                FROM Purchase.VendorRatingGrade vrg
                WHERE vrg.IsDeleted = 0
                  AND (@SearchTerm IS NULL OR vrg.GradeCode LIKE '%' + @SearchTerm + '%'
                       OR vrg.GradeName LIKE '%' + @SearchTerm + '%')";

            const string dataSql = @"
                SELECT vrg.Id, vrg.GradeCode, vrg.GradeName, vrg.MinScore, vrg.MaxScore,
                       vrg.ActionDescription, vrg.ActionTypeId, vrg.SortOrder,
                       vrg.IsActive, vrg.IsDeleted,
                       vrg.CreatedBy, vrg.CreatedDate, vrg.CreatedByName, vrg.CreatedIP,
                       vrg.ModifiedBy, vrg.ModifiedDate, vrg.ModifiedByName, vrg.ModifiedIP,
                       mm.Description AS ActionTypeName
                FROM Purchase.VendorRatingGrade vrg
                LEFT JOIN Purchase.MiscMaster mm ON vrg.ActionTypeId = mm.Id AND mm.IsDeleted = 0
                WHERE vrg.IsDeleted = 0
                  AND (@SearchTerm IS NULL OR vrg.GradeCode LIKE '%' + @SearchTerm + '%'
                       OR vrg.GradeName LIKE '%' + @SearchTerm + '%')
                ORDER BY vrg.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var totalCount = await _dbConnection.ExecuteScalarAsync<int>(countSql, new { SearchTerm = searchTerm });
            var data = (await _dbConnection.QueryAsync<VendorRatingGradeDto>(dataSql, new { SearchTerm = searchTerm, Offset = offset, PageSize = pageSize })).ToList();

            return (data, totalCount);
        }

        public async Task<VendorRatingGradeDto?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT vrg.Id, vrg.GradeCode, vrg.GradeName, vrg.MinScore, vrg.MaxScore,
                       vrg.ActionDescription, vrg.ActionTypeId, vrg.SortOrder,
                       vrg.IsActive, vrg.IsDeleted,
                       vrg.CreatedBy, vrg.CreatedDate, vrg.CreatedByName, vrg.CreatedIP,
                       vrg.ModifiedBy, vrg.ModifiedDate, vrg.ModifiedByName, vrg.ModifiedIP,
                       mm.Description AS ActionTypeName
                FROM Purchase.VendorRatingGrade vrg
                LEFT JOIN Purchase.MiscMaster mm ON vrg.ActionTypeId = mm.Id AND mm.IsDeleted = 0
                WHERE vrg.Id = @Id AND vrg.IsDeleted = 0";

            return await _dbConnection.QueryFirstOrDefaultAsync<VendorRatingGradeDto>(sql, new { Id = id });
        }

        public async Task<IReadOnlyList<VendorRatingGradeLookupDto>> AutocompleteAsync(string term, CancellationToken ct)
        {
            const string sql = @"
                SELECT Id, GradeCode, GradeName
                FROM Purchase.VendorRatingGrade
                WHERE IsActive = 1 AND IsDeleted = 0
                  AND (@Term IS NULL OR @Term = '' OR GradeCode LIKE '%' + @Term + '%'
                       OR GradeName LIKE '%' + @Term + '%')
                ORDER BY GradeName ASC";

            var result = await _dbConnection.QueryAsync<VendorRatingGradeLookupDto>(
                new CommandDefinition(sql, new { Term = term }, cancellationToken: ct));
            return result.ToList();
        }

        public async Task<bool> AlreadyExistsAsync(string gradeCode, int? id = null)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM Purchase.VendorRatingGrade
                    WHERE GradeCode = @GradeCode AND IsDeleted = 0
                      AND (@Id IS NULL OR Id != @Id)
                ) THEN 1 ELSE 0 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { GradeCode = gradeCode, Id = id });
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = @"
                SELECT CASE WHEN NOT EXISTS (
                    SELECT 1 FROM Purchase.VendorRatingGrade
                    WHERE Id = @Id AND IsDeleted = 0
                ) THEN 1 ELSE 0 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = id });
        }

        public async Task<bool> ActionTypeExistsAsync(int actionTypeId)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM Purchase.MiscMaster
                    WHERE Id = @Id AND IsDeleted = 0 AND IsActive = 1
                ) THEN 1 ELSE 0 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = actionTypeId });
        }
    }
}
