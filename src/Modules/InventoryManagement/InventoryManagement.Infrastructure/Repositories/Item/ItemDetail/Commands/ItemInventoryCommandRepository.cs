using Contracts.Interfaces;
using InventoryManagement.Application.Common.Interfaces;
using InventoryManagement.Application.Common.Interfaces.Item.ItemDetail.Commands;
using InventoryManagement.Domain.Entities.Item.ItemDetail;
using InventoryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Infrastructure.Repositories.Item.ItemDetail.Commands
{
    public class ItemInventoryCommandRepository : ItemLogRepositoryBase, IItemInventoryCommandRepository
    {
        public ItemInventoryCommandRepository(ApplicationDbContext db, IIPAddressService ipAddressService) 
            : base(db, ipAddressService) { }
        public async Task<ItemInventory?> GetByItemIdAsync(int itemId, CancellationToken ct = default)
            => await _db.ItemInventory.FirstOrDefaultAsync(x => x.ItemId == itemId, ct);

        public async Task CreateAsync(ItemInventory inventory, CancellationToken ct = default)
        {
            await _db.ItemInventory.AddAsync(inventory, ct);
            // UoW saves/commits
            var changes = DiffByReflection(new ItemInventory { ItemId = inventory.ItemId }, inventory);            
        }

        public async Task UpdateAsync(ItemInventory updated, CancellationToken ct = default)
        {
            // load tracked
            var existing = await _db.ItemInventory.FirstOrDefaultAsync(x => x.ItemId == updated.ItemId, ct);
            if (existing is null)
            {
                await CreateAsync(updated, ct);
                return;
            }

            // copy ONLY non-key scalars (do NOT touch Id / ItemId / navs)
            existing.Weight                         = updated.Weight;
            existing.WeightUomId                    = updated.WeightUomId;
            existing.DefaultMaterialRequestTypeId   = updated.DefaultMaterialRequestTypeId;
            existing.ValuationMethodId              = updated.ValuationMethodId;
            existing.ShelfLife                      = updated.ShelfLife;
            existing.UpperTolerance                 = updated.UpperTolerance;
            existing.LowerTolerance                 = updated.LowerTolerance;
            existing.BatchNumberSeries              = updated.BatchNumberSeries;
            existing.SerialNumberSeries             = updated.SerialNumberSeries;
            existing.ReorderLevel                   = updated.ReorderLevel;
            existing.ReorderQty                     = updated.ReorderQty;
            existing.RequestTypeId                  = updated.RequestTypeId;
            existing.AllowNegativeStock             = updated.AllowNegativeStock;
            existing.BatchManagement                = updated.BatchManagement;
            existing.ApplyBatchNumber               = updated.ApplyBatchNumber;

            var entry = _db.Entry(existing);
            // make extra sure keys aren’t flagged as modified
            entry.Property(x => x.Id).IsModified     = false;
            entry.Property(x => x.ItemId).IsModified = false;

            var changes = GetModifiedProps(entry);
            TryAddUpdateLog(nameof(ItemInventory), existing.ItemId, changes);
            // UoW saves/commits
        }
    }
}
