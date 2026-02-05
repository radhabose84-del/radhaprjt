using InventoryManagement.Application.Item.ItemDetail.Queries.GetAllItems;

namespace InventoryManagement.Application.Common.Interfaces.Item.ItemDetail.Commands
{
    public interface IItemManufactureCommandRepository
    {
        Task<IReadOnlyList<InventoryManagement.Domain.Entities.Item.ItemDetail.ItemManufacture>> GetByItemIdAsync(int itemId, CancellationToken ct);
        Task UpdateAsync(int itemId, IReadOnlyCollection<ItemManufactureDto> rows, CancellationToken ct);
    }
}