using Contracts.Interfaces;
using InventoryManagement.Application.Common.Interfaces.Item.ItemDetail.Commands;
using InventoryManagement.Domain.Entities.Item.ItemDetail;
using InventoryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Infrastructure.Repositories.Item.ItemDetail.Commands
{
    public class ItemSaleCommandRepository : ItemLogRepositoryBase, IItemSaleCommandRepository
    {
        public ItemSaleCommandRepository(ApplicationDbContext db, IIPAddressService ipAddressService)
            : base(db, ipAddressService) { }

        public async Task CreateAsync(ItemSale sale, CancellationToken ct = default)
        {
            await _db.ItemSale.AddAsync(sale, ct);
        }

        public async Task<ItemSale?> GetByItemIdAsync(int itemId, CancellationToken ct = default)
            => await _db.ItemSale.FirstOrDefaultAsync(x => x.ItemId == itemId, ct);

        public async Task UpdateAsync(ItemSale updated, CancellationToken ct = default)
        {
            var existing = await _db.ItemSale.FirstOrDefaultAsync(x => x.ItemId == updated.ItemId, ct);

            if (existing is null)
            {
                await CreateAsync(updated, ct);
                return;
            }

            existing.UomId = updated.UomId;
            existing.MinQuantity = updated.MinQuantity;
            existing.PackageQuantity = updated.PackageQuantity;
            existing.DeliveryLeadTime = updated.DeliveryLeadTime;
            existing.Discount = updated.Discount;
            existing.CountId = updated.CountId;
            existing.RmTypeId = updated.RmTypeId;
            existing.ValuationMethodId = updated.ValuationMethodId;

            var entry = _db.Entry(existing);
            entry.Property(x => x.Id).IsModified = false;
            entry.Property(x => x.ItemId).IsModified = false;

            var changes = GetModifiedProps(entry);
            TryAddUpdateLog(nameof(ItemSale), existing.ItemId, changes);
        }
    }
}
