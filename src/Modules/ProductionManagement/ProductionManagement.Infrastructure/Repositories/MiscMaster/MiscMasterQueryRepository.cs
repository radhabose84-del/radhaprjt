using System.Data;
using Dapper;
using ProductionManagement.Application.Common.Interfaces.IMiscMaster;
using ProductionManagement.Application.MiscMaster.Dto;

namespace ProductionManagement.Infrastructure.Repositories.MiscMaster
{
    public class MiscMasterQueryRepository : IMiscMasterQueryRepository
    {
        private readonly IDbConnection _dbConnection;

        public MiscMasterQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<(List<MiscMasterDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm, int? miscTypeId)
        {
            var offset = (pageNumber - 1) * pageSize;

            var sql = @"
                SELECT COUNT(*)
                FROM Production.MiscMaster mm
                INNER JOIN Production.MiscTypeMaster mt ON mm.MiscTypeId = mt.Id AND mt.IsDeleted = 0
                WHERE mm.IsDeleted = 0
                  AND (@MiscTypeId IS NULL OR mm.MiscTypeId = @MiscTypeId)
                  AND (@SearchTerm IS NULL OR mm.Code LIKE '%' + @SearchTerm + '%' OR mm.Description LIKE '%' + @SearchTerm + '%');

                SELECT
                    mm.Id, mm.MiscTypeId, mm.Code, mm.Description, mm.SortOrder,
                    mm.IsActive, mm.IsDeleted,
                    mm.CreatedBy, mm.CreatedDate, mm.CreatedByName, mm.CreatedIP,
                    mm.ModifiedBy, mm.ModifiedDate, mm.ModifiedByName, mm.ModifiedIP,
                    mt.MiscTypeCode, mt.Description AS MiscTypeDescription
                FROM Production.MiscMaster mm
                INNER JOIN Production.MiscTypeMaster mt ON mm.MiscTypeId = mt.Id AND mt.IsDeleted = 0
                WHERE mm.IsDeleted = 0
                  AND (@MiscTypeId IS NULL OR mm.MiscTypeId = @MiscTypeId)
                  AND (@SearchTerm IS NULL OR mm.Code LIKE '%' + @SearchTerm + '%' OR mm.Description LIKE '%' + @SearchTerm + '%')
                ORDER BY mm.MiscTypeId, mm.SortOrder
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

            using var multi = await _dbConnection.QueryMultipleAsync(sql, new
            {
                SearchTerm = string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm,
                MiscTypeId = miscTypeId,
                Offset = offset,
                PageSize = pageSize
            });

            var totalCount = await multi.ReadFirstAsync<int>();
            var data = (await multi.ReadAsync<MiscMasterDto>()).ToList();

            return (data, totalCount);
        }

        public async Task<MiscMasterDto?> GetByIdAsync(int id)
        {
            const string sql = @"
                SELECT
                    mm.Id, mm.MiscTypeId, mm.Code, mm.Description, mm.SortOrder,
                    mm.IsActive, mm.IsDeleted,
                    mm.CreatedBy, mm.CreatedDate, mm.CreatedByName, mm.CreatedIP,
                    mm.ModifiedBy, mm.ModifiedDate, mm.ModifiedByName, mm.ModifiedIP,
                    mt.MiscTypeCode, mt.Description AS MiscTypeDescription
                FROM Production.MiscMaster mm
                INNER JOIN Production.MiscTypeMaster mt ON mm.MiscTypeId = mt.Id AND mt.IsDeleted = 0
                WHERE mm.Id = @Id AND mm.IsDeleted = 0;";

            return await _dbConnection.QueryFirstOrDefaultAsync<MiscMasterDto>(sql, new { Id = id });
        }

        public async Task<IReadOnlyList<MiscMasterLookupDto>> AutocompleteAsync(string term, string? miscTypeCode, CancellationToken ct)
        {
            const string sql = @"
                SELECT mm.Id, mm.MiscTypeId, mt.MiscTypeCode, mm.Code, mm.Description
                FROM Production.MiscMaster mm
                INNER JOIN Production.MiscTypeMaster mt ON mm.MiscTypeId = mt.Id AND mt.IsDeleted = 0
                WHERE mm.IsActive = 1 AND mm.IsDeleted = 0
                  AND (@Term = '' OR mm.Code LIKE '%' + @Term + '%' OR mm.Description LIKE '%' + @Term + '%')
                  AND (@MiscTypeCode IS NULL OR mt.MiscTypeCode = @MiscTypeCode)
                ORDER BY mm.SortOrder ASC;";

            var result = await _dbConnection.QueryAsync<MiscMasterLookupDto>(sql, new
            {
                Term = term,
                MiscTypeCode = miscTypeCode
            });
            return result.ToList();
        }

        public async Task<bool> AlreadyExistsAsync(string code, int miscTypeId, int? id = null)
        {
            const string sql = @"
                SELECT COUNT(1) FROM Production.MiscMaster
                WHERE Code = @Code AND MiscTypeId = @MiscTypeId AND IsDeleted = 0
                  AND (@Id IS NULL OR Id <> @Id);";

            var count = await _dbConnection.QueryFirstAsync<int>(sql, new { Code = code, MiscTypeId = miscTypeId, Id = id });
            return count > 0;
        }

        public async Task<bool> NotFoundAsync(int id)
        {
            const string sql = @"
                SELECT COUNT(1) FROM Production.MiscMaster
                WHERE Id = @Id AND IsDeleted = 0;";

            var count = await _dbConnection.QueryFirstAsync<int>(sql, new { Id = id });
            return count == 0;
        }

        public async Task<bool> MiscTypeExistsAsync(int miscTypeId)
        {
            const string sql = @"
                SELECT COUNT(1) FROM Production.MiscTypeMaster
                WHERE Id = @Id AND IsActive = 1 AND IsDeleted = 0;";

            var count = await _dbConnection.QueryFirstAsync<int>(sql, new { Id = miscTypeId });
            return count > 0;
        }

        public async Task<ProductionManagement.Domain.Entities.MiscMaster?> GetMiscMasterByCode(string code)
        {
            const string sql = @"
                SELECT M.Id, M.Code, M.Description
                FROM Production.MiscMaster AS M
                WHERE M.IsDeleted = 0 AND M.IsActive = 1
                AND LOWER(M.Code) = LOWER(@Code);";

            return await _dbConnection.QueryFirstOrDefaultAsync<ProductionManagement.Domain.Entities.MiscMaster>(
                sql, new { Code = code });
        }

        public async Task<bool> SoftDeleteValidationAsync(int id)
        {
            const string sql = @"
                SELECT CASE WHEN
                    EXISTS (SELECT 1 FROM [Production].[CountMaster] WHERE (CountTypeId = @Id OR CountCategoryId = @Id) AND IsDeleted = 0)
                    OR EXISTS (SELECT 1 FROM [Production].[LotMaster] WHERE (LotTypeId = @Id OR StatusId = @Id) AND IsDeleted = 0)
                    OR EXISTS (SELECT 1 FROM [Production].[PackType] WHERE PackMaterialId = @Id AND IsDeleted = 0)
                    OR EXISTS (SELECT 1 FROM [Production].[ProductionPackEntry] WHERE MiscMasterId = @Id AND IsDeleted = 0)
                    OR EXISTS (SELECT 1 FROM [Production].[RepackingHeader] WHERE (FaultId = @Id OR LooseHandlingId = @Id OR WasteTypeId = @Id) AND IsDeleted = 0)
                THEN 1 ELSE 0 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = id });
        }

        public async Task<bool> IsMiscMasterLinkedAsync(int id)
        {
            const string sql = @"
                SELECT CASE WHEN
                    EXISTS (SELECT 1 FROM [Production].[CountMaster] WHERE (CountTypeId = @Id OR CountCategoryId = @Id) AND IsDeleted = 0 AND IsActive = 1)
                    OR EXISTS (SELECT 1 FROM [Production].[LotMaster] WHERE (LotTypeId = @Id OR StatusId = @Id) AND IsDeleted = 0 AND IsActive = 1)
                    OR EXISTS (SELECT 1 FROM [Production].[PackType] WHERE PackMaterialId = @Id AND IsDeleted = 0 AND IsActive = 1)
                    OR EXISTS (SELECT 1 FROM [Production].[ProductionPackEntry] WHERE MiscMasterId = @Id AND IsDeleted = 0 AND IsActive = 1)
                    OR EXISTS (SELECT 1 FROM [Production].[RepackingHeader] WHERE (FaultId = @Id OR LooseHandlingId = @Id OR WasteTypeId = @Id) AND IsDeleted = 0 AND IsActive = 1)
                THEN 1 ELSE 0 END";

            return await _dbConnection.ExecuteScalarAsync<bool>(sql, new { Id = id });
        }
    }
}
