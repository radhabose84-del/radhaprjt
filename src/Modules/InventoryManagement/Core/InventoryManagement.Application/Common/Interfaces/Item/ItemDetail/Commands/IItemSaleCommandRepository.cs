using InventoryManagement.Domain.Entities.Item.ItemDetail;

namespace InventoryManagement.Application.Common.Interfaces.Item.ItemDetail.Commands
{
    public interface IItemSaleCommandRepository
    {
        Task<ItemSale?> GetByItemIdAsync(int itemId, CancellationToken ct = default);
        Task CreateAsync(ItemSale sale, CancellationToken ct = default);
        Task UpdateAsync(ItemSale entity, CancellationToken ct = default);
    }
}
