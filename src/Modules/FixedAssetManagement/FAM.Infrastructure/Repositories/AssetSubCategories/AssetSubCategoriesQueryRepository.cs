using System.Data;
using FAM.Application.AssetSubCategories.Queries.GetAssetSubCategories;
using FAM.Application.Common.Interfaces.IAssetSubCategories;
using Dapper;

namespace FAM.Infrastructure.Repositories.AssetSubCategories
{
    public class AssetSubCategoriesQueryRepository : IAssetSubCategoriesQueryRepository
    {
        private readonly IDbConnection _dbConnection; 
          public AssetSubCategoriesQueryRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }
        public async Task<(List<AssetSubCategoriesDto>, int)> GetAllAssetSubCategoriesAsync(int PageNumber, int PageSize, string SearchTerm)
        {
              var query = $$"""
             DECLARE @TotalCount INT;
             SELECT @TotalCount = COUNT(*) 
               FROM FixedAsset.AssetSubCategories a INNER JOIN FixedAsset.AssetCategories b on a.AssetCategoriesId = b.Id
              WHERE a.IsDeleted = 0
            {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (a.SubCategoryName LIKE @Search OR b.CategoryName LIKE @Search)")}};

                SELECT 
                a.Id, 
                a.Code,
                a.SubCategoryName,
                a.Description,
                a.AssetCategoriesId,
                b.CategoryName as AssetCategoriesName,
                a.SortOrder,
                a.IsActive,
                a.CreatedDate,
                a.CreatedByName
            FROM FixedAsset.AssetSubCategories  a INNER JOIN FixedAsset.AssetCategories b on a.AssetCategoriesId = b.Id
            WHERE 
            a.IsDeleted = 0
                {{(string.IsNullOrEmpty(SearchTerm) ? "" : "AND (a.SubCategoryName LIKE @Search OR b.CategoryName LIKE @Search )")}}
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

             var assetsubCategories = await _dbConnection.QueryMultipleAsync(query, parameters);
             var assetsubcategoreplist = (await assetsubCategories.ReadAsync<AssetSubCategoriesDto>()).ToList();
             int totalCount = (await assetsubCategories.ReadFirstAsync<int>());
             return (assetsubcategoreplist, totalCount);
        }

        public async Task<List<FAM.Domain.Entities.AssetSubCategories>> GetAssetSubCategories(string searchPattern)
        {
             searchPattern = searchPattern ?? string.Empty; // Prevent null issues

            const string query = @"
             SELECT Id, SubCategoryName 
            FROM FixedAsset.AssetSubCategories 
            WHERE IsDeleted = 0 and IsActive = 1
            AND SubCategoryName LIKE @SearchPattern";  
            var parameters = new 
            { 
            SearchPattern = $"%{searchPattern}%" 
            };

            var assetSubCategories = await _dbConnection.QueryAsync<FAM.Domain.Entities.AssetSubCategories>(query, parameters);
            return assetSubCategories.ToList();
        }

        public async Task<AssetSubCategoriesDto> GetByIdAsync(int Id)
        {
             const string query = @"
                    SELECT a.*,b.CategoryName as AssetCategoriesName 
                    FROM FixedAsset.AssetSubCategories a INNER JOIN FixedAsset.AssetCategories b on a.AssetCategoriesId = b.Id  
                    WHERE a.Id = @Id AND a.IsDeleted = 0";
                    var assetSubCategories = await _dbConnection.QueryFirstOrDefaultAsync<AssetSubCategoriesDto>(query, new { Id });
                    return assetSubCategories;
        }

        public async Task<List<AssetSubCategoriesAutoCompleteDto>> GetSubcategoriesByAssetCategoryIdAsync(int AssetCategoriesId)
        {
             const string query = @"
                    SELECT 
                    b.Id,
                    b.SubCategoryName 
                    from 
                    FixedAsset.AssetCategories a INNER JOIN FixedAsset.AssetSubCategories b 
                    on a.Id=b.AssetCategoriesId 
                    and b.IsDeleted=0 and b.IsActive=1
                    and a.IsDeleted=0 
                    and a.Id=@AssetCategoriesId ";

            var assetsubCategories = await _dbConnection.QueryAsync<AssetSubCategoriesAutoCompleteDto>(query, new { AssetCategoriesId });

            return assetsubCategories.ToList(); // Ensure it returns a List
        }

        public async Task<bool> IsAssetSubCategoryLinkedAsync(int id)
        {
            const string query = @"
        SELECT TOP 1 1
        FROM [FixedAsset].[FixedAsset].[AssetMaster]
        WHERE IsDeleted = 0 AND AssetSubCategoryId = @id;
        ";

            var exists = await _dbConnection.QueryFirstOrDefaultAsync<int?>(query, new { id });
            return exists.HasValue;
        }

    }
}