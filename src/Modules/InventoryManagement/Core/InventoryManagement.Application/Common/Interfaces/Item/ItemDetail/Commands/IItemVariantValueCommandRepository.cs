using InventoryManagement.Application.Item.ItemDetail.Queries.GetAllItems;
using InventoryManagement.Domain.Entities.Item.ItemDetail.Variant;

namespace InventoryManagement.Application.Common.Interfaces.Item.ItemDetail.Commands
{
    public interface IItemVariantValueCommandRepository
    {
        Task UpsertListAsync(int itemId, IEnumerable<VariantValueDto> values, CancellationToken ct = default);
        Task MapOptionToChildAsync(int templateItemId, int attributeId, string optionValue, int childItemId, CancellationToken ct = default);
        Task AddMissingTemplateOptionsAsync(int templateItemId, IEnumerable<VariantValueDto> options, CancellationToken ct);
        Task UpsertChildSelectionsAsync(int childItemId, IEnumerable<VariantValueDto> values, CancellationToken ct = default);
        Task<List<ItemVariantValue>> GetForItemAsync(int itemId, CancellationToken ct = default);
    }
}