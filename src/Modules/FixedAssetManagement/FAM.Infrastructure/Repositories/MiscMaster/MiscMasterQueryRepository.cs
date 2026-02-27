using System.Data;
using FAM.Application.Common.Interfaces.IMiscMaster;
using Dapper;

namespace FAM.Infrastructure.Repositories.MiscMaster
{
    public class MiscMasterQueryRepository : IMiscMasterQueryRepository
    {
        private readonly IDbConnection _dbConnection;

        public MiscMasterQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;

        }

        public async Task<(List<FAM.Domain.Entities.MiscMaster>, int)> GetAllMiscMasterAsync(int PageNumber, int PageSize, string? SearchTerm)
        {
            var query = $$"""
                DECLARE @TotalCount INT;
                SELECT @TotalCount = COUNT(*) 
                FROM [FixedAsset].[MiscMaster] M
                WHERE M.IsDeleted = 0
                {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (M.Code LIKE @Search)")}}; 

                SELECT M.Id, M.MiscTypeId, M.Code, M.Description, M.SortOrder, M.IsActive, M.IsDeleted, 
                    M.CreatedBy, M.CreatedDate, M.CreatedByName, M.CreatedIP, M.ModifiedBy, M.ModifiedDate, 
                    M.ModifiedByName, M.ModifiedIP
                FROM FixedAsset.MiscMaster M
                WHERE M.IsDeleted = 0
                {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (M.Code LIKE @Search)")}}
                ORDER BY M.Id DESC 
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;
                """;

            var parameters = new
            {
                Search = $"%{SearchTerm}%",
                Offset = (PageNumber - 1) * PageSize,
                PageSize
            };

            var result = await _dbConnection.QueryMultipleAsync(query, parameters);

            // Read the data for MiscMaster and convert to list
            var miscMasterList = (await result.ReadAsync<FAM.Domain.Entities.MiscMaster>()).ToList();

            // Read the total count
            int totalCount = await result.ReadFirstAsync<int>();

            return (miscMasterList, totalCount);

        }


        public async Task<FAM.Domain.Entities.MiscMaster> GetByIdAsync(int id)
        {
            const string query = @" SELECT Id,MiscTypeId,Code,Description,SortOrder,IsActive  FROM FixedAsset.MiscMaster
             WHERE Id = @id AND IsDeleted = 0 ";
            return (await _dbConnection.QueryFirstOrDefaultAsync<FAM.Domain.Entities.MiscMaster>(query, new { id }))!;
        }

        public async Task<List<FAM.Domain.Entities.MiscMaster>> GetMiscMaster(string miscTypeCode, string miscTypeName)
        {
            const string query = @"SELECT M.Id,M.Code ,M.Description  FROM FixedAsset.MiscMaster M
            INNER JOIN [FixedAsset].[MiscTypeMaster] MT ON MT.Id = M.MiscTypeId
                WHERE M.IsDeleted = 0 AND MT.MiscTypeCode= @MiscTypeCode AND M.Code LIKE @SearchPattern ";


            var parameters = new
            {
                SearchPattern = $"%{miscTypeName ?? string.Empty}%",
                MiscTypeCode = miscTypeCode

            };

            var miscmaster = await _dbConnection.QueryAsync<FAM.Domain.Entities.MiscMaster>(query, parameters);
            return miscmaster.ToList();


        }

        public async Task<FAM.Domain.Entities.MiscMaster?> GetByMiscMasterCodeAsync(string name, int miscTypeId, int? id = null)
        {
            var query = """
                 SELECT * FROM FixedAsset.MiscMaster
                 WHERE Code = @Name AND MiscTypeId = @MiscTypeId       AND IsDeleted = 0
                 """;

            var parameters = new DynamicParameters(new { Name = name, MiscTypeId = miscTypeId });

            if (id is not null)
            {
                query += " AND Id != @Id";
                parameters.Add("Id", id);
            }

            return await _dbConnection.QueryFirstOrDefaultAsync<FAM.Domain.Entities.MiscMaster>(query, parameters);
        }

        public async Task<int> GetMaxSortOrderAsync()
        {
            var query = "SELECT ISNULL(MAX(SortOrder), 0) FROM FixedAsset.MiscMaster WHERE IsDeleted = 0";
            return await _dbConnection.QueryFirstOrDefaultAsync<int>(query);
        }
            

        public async Task<bool> AlreadyExistsAsync(string code, int miscTypeId, int? id = null)
        {
            var sql = @"
                SELECT COUNT(1)
                FROM FixedAsset.MiscMaster
                WHERE Code = @Code
                AND MiscTypeId = @MiscTypeId
                AND (@Id IS NULL OR Id != @Id)";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql, new
            {
                Code = code,
                MiscTypeId = miscTypeId,
                Id = id
            });

            return count > 0;
        }    
                    
        public async Task<FAM.Domain.Entities.MiscMaster?> GetByMiscTypeIdAndCodeAsync(int miscTypeId, string code)
            {
                var sql = @"
                    SELECT TOP 1 *
                    FROM FixedAsset.MiscMaster
                    WHERE MiscTypeId = @MiscTypeId
                    AND Code = @Code
                    AND IsDeleted = 0";

                return await _dbConnection.QueryFirstOrDefaultAsync<FAM.Domain.Entities.MiscMaster>(sql, new { MiscTypeId = miscTypeId, Code = code });
            }

        public async Task<bool> IsMiscMasterLinkedAsync(int id)
        {
            const string query = @"
        SELECT CASE WHEN

            -- AssetAmc
            EXISTS (
                SELECT 1
                FROM [FixedAsset].[FixedAsset].[AssetAmc] amc
                WHERE amc.IsDeleted = 0
                  AND (amc.CoverageType = @id OR amc.RenewalStatus = @id)
            )

            -- AssetDisposal
            OR EXISTS (
                SELECT 1
                FROM [FixedAsset].[FixedAsset].[AssetDisposal] ad
                WHERE ad.IsDeleted = 0 AND ad.DisposalType = @id
            )

            -- AssetMaster
            OR EXISTS (
                SELECT 1
                FROM [FixedAsset].[FixedAsset].[AssetMaster] am
                WHERE am.IsDeleted = 0
                  AND (am.AssetType = @id OR am.WorkingStatus = @id)
            )

            -- AssetWarranty
            OR EXISTS (
                SELECT 1
                FROM [FixedAsset].[FixedAsset].[AssetWarranty] aw
                WHERE aw.IsDeleted = 0
                  AND (aw.ServiceClaimStatus = @id OR aw.WarrantyType = @id)
            )

            -- DepreciationGroups
            OR EXISTS (
                SELECT 1
                FROM [FixedAsset].[FixedAsset].[DepreciationGroups] dg
                WHERE dg.IsDeleted = 0
                  AND (dg.BookType = @id OR dg.DepreciationMethod = @id)
            )

            -- Manufacture
            OR EXISTS (
                SELECT 1
                FROM [FixedAsset].[FixedAsset].[Manufacture] m
                WHERE m.IsDeleted = 0 AND m.ManufactureType = @id
            )

            -- UOM
            OR EXISTS (
                SELECT 1
                FROM [FixedAsset].[FixedAsset].[UOM] u
                WHERE u.IsDeleted = 0 AND u.UOMTypeId = @id
            )

        THEN 1 ELSE 0 END;
        ";

            var exists = await _dbConnection.QueryFirstOrDefaultAsync<int>(query, new { id });
            return exists == 1;
        }


    }   

    
} 