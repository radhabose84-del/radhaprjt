// IItemSupplierCommandRepository.cs
using InventoryManagement.Application.Item.ItemDetail.Queries.GetAllItems;

public interface IItemSupplierCommandRepository
{
    Task<IReadOnlyList<InventoryManagement.Domain.Entities.Item.ItemDetail.ItemSupplier>> GetByItemIdAsync(int itemId, CancellationToken ct);
    Task UpdateAsync(int itemId, IReadOnlyCollection<ItemSupplierDto> rows, CancellationToken ct);
}
