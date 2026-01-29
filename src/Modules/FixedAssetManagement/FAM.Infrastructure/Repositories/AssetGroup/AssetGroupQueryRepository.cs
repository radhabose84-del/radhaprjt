using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using FAM.Application.Common.Interfaces.IAssetGroup;
using Dapper;

namespace FAM.Infrastructure.Repositories.AssetGroup
{
    public class AssetGroupQueryRepository : IAssetGroupQueryRepository
    {
        private readonly IDbConnection _dbConnection;

        public AssetGroupQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<FAM.Domain.Entities.AssetGroup?> GetByIdAsync(int id)
        {
            const string query = @"
                    SELECT * 
                    FROM FixedAsset.AssetGroup 
                    WHERE Id = @Id AND IsDeleted = 0";

            var assetGroup = await _dbConnection.QueryFirstOrDefaultAsync<FAM.Domain.Entities.AssetGroup>(query, new { id });
            return assetGroup;
        }

        public async Task<(List<FAM.Domain.Entities.AssetGroup>, int)> GetAllAssetGroupAsync(int PageNumber, int PageSize, string? SearchTerm)
        {
            var query = $$"""
             DECLARE @TotalCount INT;
             SELECT @TotalCount = COUNT(*) 
               FROM FixedAsset.AssetGroup
              WHERE IsDeleted = 0
            {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (GroupName LIKE @Search OR Code LIKE @Search)")}};

                SELECT 
                Id, 
                Code,
                GroupName,
                SortOrder,
                IsActive,
                CreatedDate,
                CreatedByName,
                GroupPercentage     FROM FixedAsset.AssetGroup 
            WHERE 
            IsDeleted = 0
                {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (GroupName LIKE @Search OR Code LIKE @Search )")}}
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

            var assetgroup = await _dbConnection.QueryMultipleAsync(query, parameters);
            var assetgrouplist = (await assetgroup.ReadAsync<FAM.Domain.Entities.AssetGroup>()).ToList();
            int totalCount = (await assetgroup.ReadFirstAsync<int>());
            return (assetgrouplist, totalCount);
        }

        public async Task<List<FAM.Domain.Entities.AssetGroup>> GetAssetGroups(string searchPattern)
        {
            searchPattern = searchPattern ?? string.Empty; // Prevent null issues

            const string query = @"
             SELECT Id, GroupName 
            FROM FixedAsset.AssetGroup 
            WHERE IsDeleted = 0 And IsActive = 1 
            AND GroupName LIKE @SearchPattern";
            var parameters = new
            {
                SearchPattern = $"%{searchPattern}%"
            };

            var assetGroups = await _dbConnection.QueryAsync<FAM.Domain.Entities.AssetGroup>(query, parameters);
            return assetGroups.ToList();
        }
        
        public async Task<List<FAM.Domain.Entities.AssetGroup>> GetByIdsAsync(IEnumerable<int> ids)
        {
            var idList = ids?.Distinct().ToArray() ?? Array.Empty<int>();
            if (idList.Length == 0)
                return new List<FAM.Domain.Entities.AssetGroup>();

            const string query = @"
                    SELECT Id, Code, GroupName
                    FROM FixedAsset.AssetGroup
                    WHERE IsDeleted = 0
                    AND Id IN @Ids;";

            var result = await _dbConnection.QueryAsync<FAM.Domain.Entities.AssetGroup>(
                query, new { Ids = idList });

            return result.ToList();
        }

        public async Task<bool> IsAssetGroupLinkedAsync(int assetGroupId)
        {
            const string query = @"
            SELECT TOP 1 1
            FROM FixedAsset.AssetCategories
            WHERE AssetGroupId = @Id
              AND IsDeleted = 0
              AND IsActive = 1;";

            var result = await _dbConnection.QueryFirstOrDefaultAsync<int?>(
                query, new { Id = assetGroupId });
           
            return result.HasValue;
        }
    

    }
}