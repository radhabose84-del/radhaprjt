using InventoryManagement.Application.Item.ItemDetail.Queries.GetAllItems;
using InventoryManagement.Application.Item.ItemDetail.Queries.GetItemAutoComplete;

namespace InventoryManagement.Application.Common.Interfaces.Item.ItemDetail.Queries
{
    public interface IItemQueryRepository
    {
        Task<(List<ItemListDto> Items, int TotalCount)> GetAllAsync(int? page, int? size, string? search, bool onlyActive, int? itemGroupId, int? itemCategoryId, int? moduleId = null, CancellationToken ct = default);
        Task<ItemDetailsDto?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<string> GetBaseDirectoryAsync();
        Task<bool> RemoveImageReferenceAsync(string imagePath, CancellationToken ct = default);
        Task<string?> GetLatestItemCode(int itemGroupId, int itemCategoryId, CancellationToken ct = default);
        Task<List<string>> GetCandidateItemNamesAsync(string normalizedPrefix, int take = 200, CancellationToken ct = default);
        Task<List<GetItemAutoCompleteDto>> GetItemAutoCompleteAsync(string searchPattern,int? itemGroupId, int? itemCategoryId,int? sourceId,int? issueRuleId, int? moduleId = null, CancellationToken ct = default);
        Task<List<GetItemAutoCompleteDto>> GetItemsMasterByIdsAsync(IEnumerable<int> ids);          
        Task <List<ItemPurchaseToleranceDto?>> GetItemPurchaseToleranceAsync(IEnumerable<int> itemIds, CancellationToken ct = default);
        Task<List<GetItemAutoCompleteDto>> GetItemsByVariantFilterAsync(bool? hasVariant, int? parentItemId, int? moduleId = null, CancellationToken ct = default);
        Task<bool> SoftDeleteValidationAsync(int id);
        Task<bool> IsItemMasterLinkedAsync(int id);

    }
}