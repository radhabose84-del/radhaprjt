// InventoryManagement.Application/Common/Interfaces/Item/ItemDetail/Purchase/IItemPurchaseCommandRepository.cs

using InventoryManagement.Domain.Entities.Item.ItemDetail;

namespace InventoryManagement.Application.Common.Interfaces.Item.ItemDetail.Commands
{
    public interface IItemPurchaseCommandRepository
    {
        Task<ItemPurchase?> GetByItemIdAsync(int itemId, CancellationToken ct = default);
        Task CreateAsync(ItemPurchase purchase, CancellationToken ct = default);
        Task UpdateAsync(ItemPurchase entity, CancellationToken ct = default);        
    }
}
