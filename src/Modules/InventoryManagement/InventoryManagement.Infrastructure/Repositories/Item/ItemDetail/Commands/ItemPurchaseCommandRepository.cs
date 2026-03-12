using Contracts.Interfaces;
using InventoryManagement.Application.Common.Interfaces;
using InventoryManagement.Application.Common.Interfaces.Item.ItemDetail.Commands;
using InventoryManagement.Domain.Entities.Item.ItemDetail;
using InventoryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Infrastructure.Repositories.Item.ItemDetail.Commands
{
    public class ItemPurchaseCommandRepository : ItemLogRepositoryBase,IItemPurchaseCommandRepository
    {           
        public ItemPurchaseCommandRepository(ApplicationDbContext db, IIPAddressService ipAddressService)
            : base(db, ipAddressService) { }
        public async Task CreateAsync(ItemPurchase purchase, CancellationToken ct = default)
        {
            await _db.ItemPurchase.AddAsync(purchase, ct);
        }
        public async Task<ItemPurchase?> GetByItemIdAsync(int itemId, CancellationToken ct = default)
            => await _db.ItemPurchase.FirstOrDefaultAsync(x => x.ItemId == itemId, ct);        
            
        public async Task UpdateAsync(ItemPurchase updated, CancellationToken ct = default)
        {
            var existing = await _db.ItemPurchase.FirstOrDefaultAsync(x => x.ItemId == updated.ItemId, ct);

            if (existing is null)
            {
                // No row yet -> create
                await CreateAsync(updated, ct);
                return;
            }

            // ---- DO NOT change keys. Copy only non-key fields. ----
            existing.PurchaseUomId = updated.PurchaseUomId;
            existing.LeadTimeDays = updated.LeadTimeDays;
            existing.GrProcessingTimeDays = updated.GrProcessingTimeDays;
            existing.AutomaticPo = updated.AutomaticPo;
            existing.SourceOfItem = updated.SourceOfItem;            

            var entry = _db.Entry(existing);
            // Explicit: ensure keys aren’t marked modified
            entry.Property(x => x.Id).IsModified = false;
            entry.Property(x => x.ItemId).IsModified = false;

            var changes = GetModifiedProps(entry);
            TryAddUpdateLog(nameof(ItemPurchase), existing.ItemId, changes);
        }


    }
}
