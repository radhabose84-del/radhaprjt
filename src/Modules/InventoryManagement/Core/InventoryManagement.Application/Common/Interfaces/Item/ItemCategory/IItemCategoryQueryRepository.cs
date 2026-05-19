using InventoryManagement.Application.Item.ItemCategory.Queries.GetItemCategory;
using InventoryManagement.Application.Item.ItemCategory.Queries.GetItemCategoryAutoComplete;
using InventoryManagement.Application.Item.ItemCategory.Queries.Shared;

namespace InventoryManagement.Application.Common.Interfaces.Item.ItemCategory
{
    public interface IItemCategoryQueryRepository
    {
        Task<ItemCategoryDto?> GetByIdAsync(int id);
        Task<(IEnumerable<dynamic>, int)> GetAllItemCategoryAsync(int PageNumber, int PageSize, string? SearchTerm, int? moduleId);
        Task<List<ItemCategoryAutoCompleteDto>> GetItemCategoryAutoCompleteAsync(int? groupId, string searchPattern, bool isParent, int excludeId, int? moduleId, bool emergencyPo = false);
        Task<bool> SoftDeleteValidation(int Id);
        Task<List<InventoryManagement.Domain.Entities.Item.ItemCategory>> GetCategoryByIdsAsync(IEnumerable<int> ids);
        Task<bool> IsLinkedWithActiveItemsAsync(int itemCategoryId);

        Task<List<SampleQuantityDto>> GetSampleQuantitiesAsync(int itemCategoryId);
        Task<Dictionary<int, List<SampleQuantityDto>>> GetSampleQuantitiesByCategoryIdsAsync(IEnumerable<int> itemCategoryIds);
        Task<SampleQuantityDto?> GetSampleQuantityAsync(int itemCategoryId, int unitId, int uomId);
        Task<bool> UnitExistsForCategoryAsync(int itemCategoryId, int unitId, int uomId, int? excludeRowId);
        Task<bool> IsLeafCategoryAsync(int itemCategoryId);
    }
}
