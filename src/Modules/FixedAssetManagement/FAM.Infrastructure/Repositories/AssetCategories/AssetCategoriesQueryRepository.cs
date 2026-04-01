using System.Data;
using FAM.Application.AssetCategories.Queries.GetAssetCategories;
using FAM.Application.Common.Interfaces.IAssetCategories;
using Dapper;

namespace FAM.Infrastructure.Repositories.AssetCategories
{
    public class AssetCategoriesQueryRepository : IAssetCategoriesQueryRepository
    {
        private readonly IDbConnection _dbConnection; 

        public AssetCategoriesQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<(List<AssetCategoriesDto>, int)> GetAllAssetCategoriesAsync(int PageNumber, int PageSize, string? SearchTerm)
        {
             var query = $$"""
             DECLARE @TotalCount INT;
             SELECT @TotalCount = COUNT(*) 
               FROM FixedAsset.AssetCategories a INNER JOIN FixedAsset.AssetGroup b on a.AssetGroupId = b.Id
              WHERE a.IsDeleted = 0
            {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (a.CategoryName LIKE @Search OR b.GroupName LIKE @Search)")}};

                SELECT 
                a.Id, 
                a.Code,
                a.CategoryName,
                a.Description,
                a.AssetGroupId,
                b.GroupName as AssetGroupName,
                a.SortOrder,
                a.IsActive,
                a.CreatedDate,
                a.CreatedByName
            FROM FixedAsset.AssetCategories a INNER JOIN FixedAsset.AssetGroup b on a.AssetGroupId = b.Id
            WHERE 
            a.IsDeleted = 0
                {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (a.CategoryName LIKE @Search OR b.GroupName LIKE @Search )")}}
                ORDER BY a.Id desc
                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

                SELECT @TotalCount AS TotalCount;
            """;

            
             var parameters = new
                       {
                           Search = $"%{SearchTerm}%",
                           Offset = (PageNumber - 1) * PageSize,
                           PageSize
                       };

             var assetCategories = await _dbConnection.QueryMultipleAsync(query, parameters);
             var assetcategoreplist = (await assetCategories.ReadAsync<AssetCategoriesDto>()).ToList();
             int totalCount = (await assetCategories.ReadFirstAsync<int>());
             return (assetcategoreplist, totalCount);
        }

        public async Task<List<FAM.Domain.Entities.AssetCategories>> GetAssetCategories(string searchPattern)
        {
            searchPattern = searchPattern ?? string.Empty; // Prevent null issues

            const string query = @"
             SELECT Id, CategoryName 
            FROM FixedAsset.AssetCategories 
            WHERE IsDeleted = 0 and IsActive = 1
            AND CategoryName LIKE @SearchPattern";  
            var parameters = new 
            { 
            SearchPattern = $"%{searchPattern}%" 
            };

            var assetCategories = await _dbConnection.QueryAsync<FAM.Domain.Entities.AssetCategories>(query, parameters);
            return assetCategories.ToList();
        }

        public async Task<List<AssetCategoriesAutoCompleteDto?>> GetByAssetgroupIdAsync(int AssetGroupId)
        {
            const string query = @"
            SELECT 
                    b.Id,
                    b.CategoryName 
                    from 
                    FixedAsset.AssetGroup a INNER JOIN FixedAsset.AssetCategories b 
                    on a.Id=b.AssetGroupId 
                    and  b.IsDeleted=0 and b.IsActive=1
                    and a.IsDeleted=0 and a.Id=@AssetGroupId ";

            var assetCategories = await _dbConnection.QueryAsync<AssetCategoriesAutoCompleteDto>(query, new { AssetGroupId });

            return assetCategories.ToList()!; // Ensure it returns a List
        }

    

        public async Task<AssetCategoriesDto?> GetByIdAsync(int Id)
        {
            const string query = @"
                    SELECT a.*,b.GroupName as AssetGroupName 
                    FROM FixedAsset.AssetCategories a inner join FixedAsset.AssetGroup b on a.AssetGroupId = b.Id 
                    WHERE a.Id = @Id AND a.IsDeleted = 0";
                    var assetCategories = await _dbConnection.QueryFirstOrDefaultAsync<AssetCategoriesDto>(query, new { Id });
                    return assetCategories;
        }

        public async Task<bool> IsAssetCategoryLinkedAsync(int categoryId)
        {
            const string query = @"
        SELECT TOP 1 1
        FROM [FixedAsset].[AssetSubCategories]
        WHERE IsDeleted = 0 AND IsActive = 1 AND AssetCategoriesId = @categoryId;
        ";

            var result = await _dbConnection.QueryFirstOrDefaultAsync<int?>(query, new { categoryId });
            return result.HasValue; 
        }
    
    
        public async Task<bool> SoftDeleteValidationAsync(int id)
        {
            const string query = @"
        SELECT CASE WHEN
            EXISTS (
                SELECT 1
                FROM [FixedAsset].[AssetSubCategories]
                WHERE IsDeleted = 0 AND AssetCategoriesId = @id
            )
            OR EXISTS (
                SELECT 1
                FROM [FixedAsset].[AssetMaster]
                WHERE IsDeleted = 0 AND AssetCategoryId = @id
            )
        THEN 1 ELSE 0 END;";

            return await _dbConnection.ExecuteScalarAsync<bool>(query, new { id });
        }
    }
}
