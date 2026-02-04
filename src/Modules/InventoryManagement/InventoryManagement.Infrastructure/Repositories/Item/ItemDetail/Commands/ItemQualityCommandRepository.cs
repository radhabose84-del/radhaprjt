using InventoryManagement.Application.Common.Interfaces;
using InventoryManagement.Application.Common.Interfaces.Item.ItemDetail.Commands;
using InventoryManagement.Domain.Entities.Item.ItemDetail;
using InventoryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Infrastructure.Repositories.Item.ItemDetail.Commands
{
    public class ItemQualityCommandRepository : ItemLogRepositoryBase, IItemQualityCommandRepository
    {        
        public ItemQualityCommandRepository(ApplicationDbContext db, IIPAddressService ipAddressService)
            : base(db, ipAddressService) { }

        public async Task<ItemQuality?> GetByItemIdAsync(int itemId, CancellationToken ct = default)
            => await _db.ItemQuality.FirstOrDefaultAsync(x => x.ItemId == itemId, ct);

        public async Task CreateAsync(ItemQuality quality, CancellationToken ct = default)
        {
            await _db.ItemQuality.AddAsync(quality, ct);
            var changes = DiffByReflection(new ItemQuality { ItemId = quality.ItemId }, quality);
        }

        public async Task UpdateAsync(ItemQuality updated, CancellationToken ct = default)
        {
            var existing = await _db.ItemQuality.FirstOrDefaultAsync(x => x.ItemId == updated.ItemId, ct);
            if (existing is null)
            {
                await CreateAsync(updated, ct);
                return;
            }

            // copy ONLY non-key scalars (never touch Id/ItemId)
            existing.InspectionTemplateId = updated.InspectionTemplateId;
            existing.CertificateTypeId = updated.CertificateTypeId;
            existing.InspLotProcessingTime = updated.InspLotProcessingTime;
            existing.InspectionRequired = updated.InspectionRequired;
            existing.QualityInspectionFree = updated.QualityInspectionFree;
            existing.IsCertificateRequiredFromSupplier = updated.IsCertificateRequiredFromSupplier;

            var entry = _db.Entry(existing);
            entry.Property(x => x.Id).IsModified = false;
            entry.Property(x => x.ItemId).IsModified = false;

            var changes = GetModifiedProps(entry);
            TryAddUpdateLog(nameof(ItemQuality), existing.ItemId, changes);
        }    
    }
}
