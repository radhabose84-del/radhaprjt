using InventoryManagement.Domain.Entities.Item.ItemDetail;

namespace InventoryManagement.Application.Common.Interfaces.Item.ItemDetail.Commands
{
    public interface IItemQualityCommandRepository
    {
        Task<ItemQuality?> GetByItemIdAsync(int itemId, CancellationToken ct = default);
        Task CreateAsync(ItemQuality quality, CancellationToken ct = default);
        Task UpdateAsync(ItemQuality entity, CancellationToken ct = default);               
    }
}
