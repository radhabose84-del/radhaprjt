using FAM.Application.AssetCategories.Queries.GetAssetCategories;

namespace FAM.Application.Common.Interfaces.IAssetCategories
{
    public interface IAssetCategoriesQueryRepository
    {
        Task<AssetCategoriesDto?> GetByIdAsync(int Id);
        Task<(List<AssetCategoriesDto>, int)> GetAllAssetCategoriesAsync(int PageNumber, int PageSize, string? SearchTerm);
        Task<List<FAM.Domain.Entities.AssetCategories>> GetAssetCategories(string searchPattern);
        Task<List<AssetCategoriesAutoCompleteDto?>> GetByAssetgroupIdAsync(int AssetGroupId);
        Task<bool> IsAssetCategoryLinkedAsync(int assetCategoryId); //IsActive And Delete Validation

    }
}