using InventoryManagement.Application.Common.Interfaces;
using InventoryManagement.Application.Common.Interfaces.Item.ItemDetail.Commands;
using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Entities.Item.ItemDetail;
using InventoryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Infrastructure.Repositories.Item.ItemDetail.Commands
{
    public sealed class ItemCommandRepository : ItemLogRepositoryBase, IItemCommandRepository
    {
        private readonly IIPAddressService _ipAddressService;

        public ItemCommandRepository(ApplicationDbContext db, IIPAddressService ipAddressService)
            : base(db, ipAddressService)
        {
            _ipAddressService = ipAddressService;
        }

        public async Task<int> CreateAsync(ItemMaster item, CancellationToken ct = default)
        {
            await _db.ItemMaster.AddAsync(item, ct);
            await _db.SaveChangesAsync(ct);
            return item.Id;
        }

        public async Task UpdateAsync(ItemMaster entity, CancellationToken ct = default)
        {
            var entry = _db.Entry(entity);
            List<PropertyChange> changes;

            if (entry.State == EntityState.Detached)
            {
                var original = await _db.ItemMaster.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == entity.Id, ct)
                    ?? throw new KeyNotFoundException("Item not found.");

                changes = DiffByReflection(original, entity);

                _db.ItemMaster.Attach(entity);
                entry = _db.Entry(entity);
                entry.State = EntityState.Modified;
                entry.Property(x => x.Id).IsModified = false;
            }
            else
            {
                changes = GetModifiedProps(entry);
            }
            entity.ModifiedDate = DateTimeOffset.UtcNow;
            TryAddUpdateLog(nameof(ItemMaster), entity.Id, changes);
        }

        public Task<ItemMaster?> GetTrackingAsync(int id, CancellationToken ct = default) =>
            _db.ItemMaster.FirstOrDefaultAsync(x => x.Id == id, ct);

        public Task<bool> ExistsByCodeForUpdateAsync(string itemCode, int excludeId, CancellationToken ct = default) =>
            _db.ItemMaster.AnyAsync(x => x.ItemCode == itemCode && x.Id != excludeId && x.IsActive == BaseEntity.Status.Active, ct);

        public Task<bool> ExistsByCodeForCreateAsync(string itemCode, CancellationToken ct = default) =>
            _db.ItemMaster.AnyAsync(x => x.ItemCode == itemCode && x.IsDeleted == BaseEntity.IsDelete.NotDeleted, ct);

        public Task<List<int>> GetChildIdsAsync(int templateItemId, CancellationToken ct = default) =>
            _db.ItemMaster
               .Where(x => x.ParentItemId == templateItemId && x.IsDeleted == BaseEntity.IsDelete.NotDeleted)
               .Select(x => x.Id)
               .ToListAsync(ct);

        public async Task<bool> UpdateItemImageAsync(int itemId, string imageName, CancellationToken ct = default)
        {
            var asset = await _db.ItemMaster.FindAsync(new object[] { itemId }, ct);
            if (asset is null) return false;
            asset.ItemImage = imageName;
            await _db.SaveChangesAsync(ct);
            return true;
        }

        // ---------------- smart name duplicate checks (EF-translatable) ----------------
        public async Task<bool> ExistsByNameSmartForCreateAsync(string name, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;

            var normInput = NormalizeClient(name);

            // Try SQL Server server-side normalization only when actually on SQL Server
            if (_db.Database.IsSqlServer())
            {
                try
                {
                    return await _db.ItemMaster
                        .AsNoTracking()
                        .Where(i => i.IsDeleted == BaseEntity.IsDelete.NotDeleted && i.ItemName != null)
                        .AnyAsync(i =>
                            EF.Functions.Collate(i.ItemName!, "Latin1_General_CI_AI")
                                .ToLower()
                                .Replace(" ", "")
                                .Replace("-", "")
                                .Replace("_", "")
                                .Replace(".", "")
                                .Replace("/", "")
                                .Replace("\\", "")
                                .Replace("'", "")
                                .Replace("(", "")
                                .Replace(")", "") == normInput, ct);
                }
                catch (Exception ex) when (ex is NotSupportedException || ex is NotImplementedException)
                {
                    // fall through to fallback
                }
            }

            // Fallback (provider-agnostic): coarse server filter + client normalization
            var token = normInput.Length >= 4 ? normInput.Substring(0, 4) : normInput;
            var candidates = await _db.ItemMaster
                .AsNoTracking()
                .Where(i => i.IsDeleted == BaseEntity.IsDelete.NotDeleted && i.ItemName != null)
                .Where(i => i.ItemName!.Contains(token))      // coarse narrowing
                .OrderByDescending(i => i.Id)
                .Select(i => i.ItemName!)
                .Take(1000)
                .ToListAsync(ct);

            return candidates.Select(NormalizeClient).Any(n => n == normInput);
        }


        public Task<bool> ExistsByNameSmartForUpdateAsync(string name, int excludeId, CancellationToken ct = default)
        {
            var normInput = NormalizeClient(name);

            return _db.ItemMaster
                .AsNoTracking()
                .Where(i => i.Id != excludeId && i.IsDeleted == BaseEntity.IsDelete.NotDeleted)
                .Select(i =>
                    EF.Functions.Collate(i.ItemName ?? "", "Latin1_General_CI_AI")
                        .ToLower()
                        .Replace(" ", "")
                        .Replace("-", "")
                        .Replace("_", "")
                        .Replace(".", "")
                        .Replace("/", "")
                        .Replace("\\", "")
                        .Replace("'", "")
                        .Replace("(", "")
                        .Replace(")", ""))
                .AnyAsync(norm => norm == normInput, ct);
        }

        // client-side only
        private static string NormalizeClient(string? s) =>
            (s ?? string.Empty).ToLowerInvariant()
                .Replace(" ", "")
                .Replace("-", "")
                .Replace("_", "")
                .Replace(".", "")
                .Replace("/", "")
                .Replace("\\", "")
                .Replace("'", "")
                .Replace("(", "")
                .Replace(")", "");
    }
}
