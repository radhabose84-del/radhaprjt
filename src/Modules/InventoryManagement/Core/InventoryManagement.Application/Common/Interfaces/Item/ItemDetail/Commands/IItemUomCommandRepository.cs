using InventoryManagement.Application.Item.ItemDetail.Queries.GetAllItems;

namespace InventoryManagement.Application.Common.Interfaces.Item.ItemDetail.Commands
{
    public interface IItemUomCommandRepository
    {
        Task<IReadOnlyList<InventoryManagement.Domain.Entities.Item.ItemDetail.ItemUOM>> GetByItemIdAsync(int itemId, CancellationToken ct);
        Task UpdateAsync(int itemId, IReadOnlyCollection<ItemUomDto> rows, CancellationToken ct);
    }
}
