using InventoryManagement.Application.Item.ItemDetail.Queries.GetAllItems;
using InventoryManagement.Domain.Entities.Item.ItemDetail;

public interface IItemUnitMappingCommandRepository
{
    Task<IReadOnlyList<ItemUnitMapping>> GetByItemIdAsync(int itemId, CancellationToken ct);
    Task CreateAsync(ItemUnitMapping entity, CancellationToken ct);
    Task UpdateAsync(int itemId, IReadOnlyCollection<ItemUnitMappingDto> rows, CancellationToken ct);
}
