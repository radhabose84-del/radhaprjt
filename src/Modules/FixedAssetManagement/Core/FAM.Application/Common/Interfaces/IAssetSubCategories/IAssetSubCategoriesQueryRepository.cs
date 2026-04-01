using FAM.Application.AssetSubCategories.Queries.GetAssetSubCategories;

namespace FAM.Application.Common.Interfaces.IAssetSubCategories
{
    public interface IAssetSubCategoriesQueryRepository
    {
        Task<AssetSubCategoriesDto?> GetByIdAsync(int Id);
        Task<(List<AssetSubCategoriesDto>, int)> GetAllAssetSubCategoriesAsync(int PageNumber, int PageSize, string? SearchTerm);
        Task<List<FAM.Domain.Entities.AssetSubCategories>> GetAssetSubCategories(string searchPattern);
        Task<List<AssetSubCategoriesAutoCompleteDto?>> GetSubcategoriesByAssetCategoryIdAsync(int AssetCategoriesId);
        Task<bool> IsAssetSubCategoryLinkedAsync(int id);
        Task<bool> SoftDeleteValidationAsync(int id);
    }
}