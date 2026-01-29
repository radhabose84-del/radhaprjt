using System.Data;
using FAM.Application.Common.Interfaces.IAssetSubGroup;
using Dapper;

namespace FAM.Infrastructure.Repositories.AssetSubGroup
{
    public class AssetSubGroupQueryRepository : IAssetSubGroupQueryRepository
    {
        private readonly IDbConnection _dbConnection; 

        public AssetSubGroupQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<FAM.Domain.Entities.AssetSubGroup?> GetByIdAsync( int id)
        {
                const string query = @"
                    SELECT * 
                    FROM FixedAsset.AssetSubGroup 
                    WHERE Id = @Id AND IsDeleted = 0";

                    var assetSubGroup = await _dbConnection.QueryFirstOrDefaultAsync<FAM.Domain.Entities.AssetSubGroup>(query, new { id });
                    return assetSubGroup;
        }
        public async Task<List<FAM.Domain.Entities.AssetSubGroup?>> GetByGroupIdAsync(int groupId)
        {
            const string query = @"
                    SELECT * 
                    FROM FixedAsset.AssetSubGroup 
                    WHERE GroupId = @groupId AND IsDeleted = 0";

            var assetSubGroup = await _dbConnection.QueryAsync<FAM.Domain.Entities.AssetSubGroup>(query, new { groupId });
            return assetSubGroup.ToList();             
        }

         public async Task<(List<FAM.Domain.Entities.AssetSubGroup>, int)> GetAllAssetSubGroupAsync(int PageNumber, int PageSize, string? SearchTerm)
        {
            var query = $$"""
            DECLARE @TotalCount INT;
            SELECT @TotalCount = COUNT(*) 
               FROM FixedAsset.AssetSubGroup
              WHERE IsDeleted = 0
            {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (SubGroupName LIKE @Search OR Code LIKE @Search)")}};

                SELECT 
                Id, 
                Code,
                SubGroupName,
                SortOrder,
                GroupId,AdditionalDepreciation,SubGroupPercentage,
                IsActive,
                CreatedDate,
                CreatedByName
            FROM FixedAsset.AssetSubGroup 
            WHERE 
            IsDeleted = 0
                {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (SubGroupName LIKE @Search OR Code LIKE @Search )")}}
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

            var assetSubGroup = await _dbConnection.QueryMultipleAsync(query, parameters);
            var assetSubGroupList = (await assetSubGroup.ReadAsync<FAM.Domain.Entities.AssetSubGroup>()).ToList();
            int totalCount = (await assetSubGroup.ReadFirstAsync<int>());
            return (assetSubGroupList, totalCount);
        }

        public async Task<List<FAM.Domain.Entities.AssetSubGroup>> GetAssetSubGroups(string searchPattern)
        {
            searchPattern = searchPattern ?? string.Empty; // Prevent null issues

            const string query = @"
             SELECT Id, SubGroupName ,GroupId
            FROM FixedAsset.AssetSubGroup 
            WHERE IsDeleted = 0 
            AND SubGroupName LIKE @SearchPattern";  
            var parameters = new 
            { 
            SearchPattern = $"%{searchPattern}%" 
            };
            var assetSubGroups = await _dbConnection.QueryAsync<FAM.Domain.Entities.AssetSubGroup>(query, parameters);
            return assetSubGroups.ToList();
        }

        public async Task<bool> IsAssetSubGroupLinkedAsync(int id)
        {
            const string query = @"
        SELECT CASE WHEN
            EXISTS (
                SELECT 1
                FROM [FixedAsset].[FixedAsset].[AssetMaster] am
                WHERE am.IsDeleted = 0 AND am.AssetSubGroupId = @id
            )
            OR EXISTS (
                SELECT 1
                FROM [FixedAsset].[FixedAsset].[WDVDepreciationDetail] wdv
                WHERE wdv.AssetSubGroupId = @id
            )
        THEN 1 ELSE 0 END;
        ";

            var exists = await _dbConnection.QueryFirstOrDefaultAsync<int>(query, new { id });
            return exists == 1;
        }
    }
}