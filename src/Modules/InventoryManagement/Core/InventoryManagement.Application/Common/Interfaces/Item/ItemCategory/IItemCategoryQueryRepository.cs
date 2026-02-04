using InventoryManagement.Application.Item.ItemCategory.Queries.GetItemCategory;
using InventoryManagement.Application.Item.ItemCategory.Queries.GetItemCategoryAutoComplete;

namespace InventoryManagement.Application.Common.Interfaces.Item.ItemCategory
{
    public interface IItemCategoryQueryRepository
    {
        Task<ItemCategoryDto> GetByIdAsync(int id);
        Task<(IEnumerable<dynamic>, int)> GetAllItemCategoryAsync(int PageNumber, int PageSize, string? SearchTerm);
        Task<List<ItemCategoryAutoCompleteDto>> GetItemCategoryAutoCompleteAsync(int? groupId, string searchPattern, bool isParent = false,int excludeId = 0);
        Task<bool> SoftDeleteValidation(int Id);
        Task<List<InventoryManagement.Domain.Entities.Item.ItemCategory>> GetCategoryByIdsAsync(IEnumerable<int> ids);  
        Task<bool> IsLinkedWithActiveItemsAsync(int itemCategoryId);    
    }
}