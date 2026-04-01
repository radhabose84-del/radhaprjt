using System.Data;
using Contracts.Interfaces.Validations.MaintenanceManagement;
using FAM.Application.Common.Interfaces.IUOM;
using FAM.Application.UOM.Queries.GetUOMTypeAutoComplete;
using FAM.Domain.Entities;
using Dapper;

namespace FAM.Infrastructure.Repositories.UOMs
{
    public class UOMQueryRepository : IUOMQueryRepository
    {
        private readonly IDbConnection _dbConnection;
        private readonly IMaintenanceUomValidation _uomValidation;

        public UOMQueryRepository(IDbConnection dbConnection, IMaintenanceUomValidation uomValidation)
        {
            _dbConnection = dbConnection;
            _uomValidation = uomValidation;
        }        
        public async Task<(List<UOM>, int)> GetAllUOMAsync(int PageNumber, int PageSize, string? SearchTerm)
        {
            var query = $$"""
             DECLARE @TotalCount INT;
             SELECT @TotalCount = COUNT(*) 
               FROM FixedAsset.UOM 
              WHERE IsDeleted = 0
            {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (Code LIKE @Search OR UOMName LIKE @Search)")}};

                SELECT 
                Id, 
                Code,
                UOMName,
                SortOrder,
                UOMTypeId,
                IsActive,
                CreatedDate,
                CreatedByName
            FROM FixedAsset.UOM 
            WHERE 
            IsDeleted = 0
                {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (Code LIKE @Search OR UOMName LIKE @Search )")}}
                ORDER BY Id DESC
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;
            """;

            
             var parameters = new
                       {
                           Search = $"%{SearchTerm}%",
                           Offset = (PageNumber - 1) * PageSize,
                           PageSize
                       };

            var uom = await _dbConnection.QueryMultipleAsync(query, parameters);
            var uomlist = (await uom.ReadAsync<UOM>()).ToList();
            int totalCount = (await uom.ReadFirstAsync<int>());
            return (uomlist, totalCount);
        }

        public async Task<UOM> GetByIdAsync(int id)
        {
            const string query = "SELECT * FROM FixedAsset.UOM WHERE Id = @Id AND IsDeleted = 0";
            return (await _dbConnection.QueryFirstOrDefaultAsync<UOM>(query, new { id }))!;
        }

        public async Task<UOM?> GetByUOMNameAsync(string name, int? id = null)
        {
            var query = """
                 SELECT * FROM FixedAsset.UOM
                 WHERE UOMName = @UOMName AND IsDeleted = 0
                 """;

             var parameters = new DynamicParameters(new { UOMName = name });

             if (id is not null)
             {
                 query += " AND Id != @Id";
                 parameters.Add("Id", id);
             }

            return await _dbConnection.QueryFirstOrDefaultAsync<UOM>(query, parameters);
        }
        public async Task<List<UOM>> GetUOM(string? searchPattern=null)
        {
            const string query = @"
                SELECT Id, UOMName 
                FROM FixedAsset.UOM 
                WHERE IsDeleted = 0 AND UOMName LIKE @SearchPattern";
                
            var uoms = await _dbConnection.QueryAsync<UOM>(query, new { SearchPattern = $"%{searchPattern}%" });
            return uoms.ToList();
        }

        public async Task<List<UOMTypeAutoCompleteDto>> GetUOMType(string searchPattern)
        {
                const string query = @"
                SELECT DISTINCT mm.Id, CAST(mm.Code AS NVARCHAR(255)) AS UomType
                FROM FixedAsset.UOM um
                INNER JOIN FixedAsset.MiscMaster mm ON um.UOMTypeId = mm.MiscTypeId
                WHERE um.IsDeleted = 0 AND mm.Code LIKE @SearchPattern";
                
            var uoms = await _dbConnection.QueryAsync<UOMTypeAutoCompleteDto>(query, new { SearchPattern = $"%{searchPattern}%" });
            return uoms.ToList();
        }
        public async Task<bool> IsUomLinkedAsync(int uomId)
            => await _uomValidation.HasActiveUomAsync(uomId);

        public async Task<bool> SoftDeleteValidationAsync(int id)
        {
            // Check same-module FAM reference
            const string sql = @"
        SELECT CASE WHEN EXISTS (
            SELECT 1 FROM [FixedAsset].[AssetMaster]
            WHERE IsDeleted = 0 AND UOMId = @id
        ) THEN 1 ELSE 0 END;";

            var famLinked = await _dbConnection.ExecuteScalarAsync<bool>(sql, new { id });
            if (famLinked) return true;

            // Check cross-module Maintenance reference via validation interface
            return await _uomValidation.HasLinkedUomAsync(id);
        }
    }
}
