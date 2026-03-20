using InventoryManagement.Application.Item.ItemDetail.Queries.GetAllItems;
using InventoryManagement.Domain.Entities.Item.ItemDetail;

public interface IItemUsageTypeMappingCommandRepository
{
    Task<IReadOnlyList<ItemUsageTypeMapping>> GetByItemIdAsync(int itemId, CancellationToken ct);
    Task CreateAsync(ItemUsageTypeMapping entity, CancellationToken ct);
    Task UpdateAsync(int itemId, IReadOnlyCollection<ItemUsageTypeMappingDto> rows, CancellationToken ct);
}
