using InventoryManagement.Application.Item.ItemDetail.Queries.GetAllItems;

namespace InventoryManagement.Application.Common.Interfaces.Item.ItemDetail.Commands
{
    public interface IItemVariantAttributeCommandRepository
    {
        Task UpsertAttributesAsync(int itemId, List<VariantAttributeDto> attrs, CancellationToken ct = default);
        Task<List<VariantAttributeDto>> GetForItemAsync(int itemId, CancellationToken ct = default);
        Task AddMissingTemplateOptionsAsync(int templateItemId,IEnumerable<VariantValueDto> options, CancellationToken ct = default);
    }
}