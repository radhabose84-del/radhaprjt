using System.Data;
using Contracts.Interfaces.Lookups.Party;
using Dapper;
using PurchaseManagement.Application.Common.Interfaces.IVendorEvaluationHeader;
using PurchaseManagement.Application.VendorEvaluationHeader.Dto;

namespace PurchaseManagement.Infrastructure.Repositories.VendorEvaluationHeader
{
    public class VendorEvaluationHeaderQueryRepository : IVendorEvaluationHeaderQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly ISupplierLookup _supplierLookup;

        public VendorEvaluationHeaderQueryRepository(IDbConnection dbConnection, ISupplierLookup supplierLookup)
        {
            _dbConnection = dbConnection;
            _supplierLookup = supplierLookup;
        }

        public async Task<(List<VendorEvaluationHeaderDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm)
        {
            var offset = (pageNumber - 1) * pageSize;

            const string countSql = @"
                SELECT COUNT(*)
                FROM Purchase.VendorEvaluationHeader veh
                WHERE veh.IsDeleted = 0
                  AND (@SearchTerm IS NULL OR veh.EvaluationCode LIKE '%' + @SearchTerm + '%'
                       OR veh.Remarks LIKE '%' + @SearchTerm + '%')";

            const string dataSql = @"
                SELECT veh.Id, veh.EvaluationCode, veh.VendorId,
                       veh.EvaluationMonth, veh.EvaluationYear, veh.EvaluationDate,
                       veh.TotalWeightedScore, veh.GradeId, veh.Remarks,
                       veh.IsActive, veh.IsDeleted,
                       veh.CreatedBy, veh.CreatedDate, veh.CreatedByName,
                       veh.ModifiedBy, veh.ModifiedDate, veh.ModifiedByName,
                       vrg.GradeCode, vrg.GradeName
                FROM Purchase.VendorEvaluationHeader veh
                LEFT JOIN Purchase.VendorRatingGrade vrg ON veh.GradeId = vrg.Id AND vrg.IsDeleted = 0
                WHERE veh.IsDeleted = 0
                  AND (@SearchTerm IS NULL OR veh.EvaluationCode LIKE '%' + @SearchTerm + '%'
                       OR veh.Remarks LIKE '%' + @SearchTerm + '%')
                ORDER BY veh.Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var totalCount = await _dbConnection.ExecuteScalarAsync<int>(countSql, new { SearchTerm = searchTerm });
            var data = (await _dbConnection.QueryAsync<VendorEvaluationHeaderDto>(dataSql, new { SearchTerm = searchTerm, Offset = offset, PageSize = pageSize })).ToList();

            // Populate VendorName via cross-module lookup
            var vendorIds = data.Where(d => d.VendorId > 0).Select(d => d.VendorId).Distinct().ToList();
            if (vendorIds.Count > 0)
            {
                var vendorDict = new Dictionary<int, string>();
                foreach (var vendorId in vendorIds)
                {
                    var supplier = await _supplierLookup.GetActiveSupplierByIdAsync(vendorId);
                    if (supplier != null)
                    {
                        vendorDict[vendorId] = supplier.VendorName;
                    }
                }

                foreach (var item in data)
                {
                    if (vendorDict.TryGetValue(item.VendorId, out var vendorName))
                    {
                        item.VendorName = vendorName;
                    }
                }
            }

            return (data, totalCount);
        }

        public async Task<VendorEvaluationHeaderDto?> GetByIdAsync(int id)
        {
            const string headerSql = @"
                SELECT veh.Id, veh.EvaluationCode, veh.VendorId,
                       veh.EvaluationMonth, veh.EvaluationYear, veh.EvaluationDate,
                       veh.TotalWeightedScore, veh.GradeId, veh.Remarks,
                       veh.IsActive, veh.IsDeleted,
                       veh.CreatedBy, veh.CreatedDate, veh.CreatedByName,
                       veh.ModifiedBy, veh.ModifiedDate, veh.ModifiedByName,
                       vrg.GradeCode, vrg.GradeName
                FROM Purchase.VendorEvaluationHeader veh
                LEFT JOIN Purchase.VendorRatingGrade vrg ON veh.GradeId = vrg.Id AND vrg.IsDeleted = 0
                WHERE veh.Id = @Id AND veh.IsDeleted = 0";

            const string detailSql = @"
                SELECT ved.Id, ved.VendorEvaluationHeaderId, ved.CriteriaId,
                       ved.Score, ved.WeightagePercent, ved.WeightedScore,
                       ved.ScoringMethod, ved.Remarks,
                       vec.CriteriaCode, vec.CriteriaName
                FROM Purchase.VendorEvaluationDetail ved
                LEFT JOIN Purchase.VendorEvaluationCriteria vec ON ved.CriteriaId = vec.Id AND vec.IsDeleted = 0
                WHERE ved.VendorEvaluationHeaderId = @Id AND ved.IsDeleted = 0
                ORDER BY ved.Id ASC";

            var header = await _dbConnection.QueryFirstOrDefaultAsync<VendorEvaluationHeaderDto>(headerSql, new { Id = id });
            if (header == null) return null;

            // Populate VendorName via cross-module lookup
            var supplier = await _supplierLookup.GetActiveSupplierByIdAsync(header.VendorId);
            if (supplier != null)
            {
                header.VendorName = supplier.VendorName;
            }

            // Load child details
            var details = (await _dbConnection.QueryAsync<VendorEvaluationDetailDto>(detailSql, new { Id = id })).ToList();
            header.VendorEvaluationDetails = details;

            return header;
        }

        public async Task<bool> CompositeKeyExistsAsync(int vendorId, int evaluationMonth, int evaluationYear, int? id = null)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM Purchase.VendorEvaluationHeader
                    WHERE VendorId = @VendorId
                      AND EvaluationMonth = @EvaluationMonth
                      AND EvaluationYear = @EvaluationYear
                      AND IsDeleted = 0
                      AND (@Id IS NULL OR Id != @Id)
                ) THEN 1 ELSE 0 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { VendorId = vendorId, EvaluationMonth = evaluationMonth, EvaluationYear = evaluationYear, Id = id });
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = @"
                SELECT CASE WHEN NOT EXISTS (
                    SELECT 1 FROM Purchase.VendorEvaluationHeader
                    WHERE Id = @Id AND IsDeleted = 0
                ) THEN 1 ELSE 0 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = id });
        }

        public async Task<bool> VendorExistsAsync(int vendorId)
        {
            var supplier = await _supplierLookup.GetActiveSupplierByIdAsync(vendorId);
            return supplier != null;
        }

        public async Task<bool> GradeExistsAsync(int gradeId)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM Purchase.VendorRatingGrade
                    WHERE Id = @Id AND IsDeleted = 0 AND IsActive = 1
                ) THEN 1 ELSE 0 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = gradeId });
        }

        public async Task<bool> CriteriaExistsAsync(int criteriaId)
        {
            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 FROM Purchase.VendorEvaluationCriteria
                    WHERE Id = @Id AND IsDeleted = 0 AND IsActive = 1
                ) THEN 1 ELSE 0 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = criteriaId });
        }
    }
}
