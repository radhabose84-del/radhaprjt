using Contracts.Interfaces;
using InventoryManagement.Application.Item.ItemDetail.Queries.GetAllItems;
using InventoryManagement.Domain.Entities.Item.ItemDetail;
using InventoryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Infrastructure.Repositories.Item.ItemDetail.Commands
{
    public sealed class ItemUsageTypeMappingCommandRepository : ItemLogRepositoryBase, IItemUsageTypeMappingCommandRepository
    {
        public ItemUsageTypeMappingCommandRepository(ApplicationDbContext db, IIPAddressService ipAddressService)
            : base(db, ipAddressService) { }

        public async Task<IReadOnlyList<ItemUsageTypeMapping>> GetByItemIdAsync(int itemId, CancellationToken ct)
        {
            var list = await _db.ItemUsageTypeMapping
                .Where(x => x.ItemId == itemId)
                .ToListAsync(ct);
            return list;
        }

        public async Task CreateAsync(ItemUsageTypeMapping entity, CancellationToken ct)
        {
            await _db.ItemUsageTypeMapping.AddAsync(entity, ct);
            await _db.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(int itemId, IReadOnlyCollection<ItemUsageTypeMappingDto> rows, CancellationToken ct)
        {
            // Normalize incoming payload
            var incoming = (rows ?? Array.Empty<ItemUsageTypeMappingDto>())
                .Where(r => r is not null && r.UsageTypeId > 0 && r.UnitId > 0)
                .Select(r => new ItemUsageTypeMapping
                {
                    ItemId = itemId,
                    UsageTypeId = r.UsageTypeId,
                    UnitId = r.UnitId
                })
                .ToList();

            // Load existing tracked rows
            var existing = await _db.ItemUsageTypeMapping
                .Where(x => x.ItemId == itemId)
                .ToListAsync(ct);

            // Build key sets (UsageTypeId, UnitId)
            var incomingKeys = new HashSet<(int UsageTypeId, int UnitId)>(
                incoming.Select(i => (i.UsageTypeId, i.UnitId)));

            var existingByKey = existing.ToDictionary(e => (e.UsageTypeId, e.UnitId));

            // INSERT new rows
            foreach (var inc in incoming)
            {
                if (!existingByKey.ContainsKey((inc.UsageTypeId, inc.UnitId)))
                {
                    await _db.ItemUsageTypeMapping.AddAsync(inc, ct);
                }
            }

            // DELETE rows not in payload
            foreach (var cur in existing)
            {
                var key = (cur.UsageTypeId, cur.UnitId);
                if (!incomingKeys.Contains(key))
                {
                    _db.ItemUsageTypeMapping.Remove(cur);
                }
            }
        }
    }
}
