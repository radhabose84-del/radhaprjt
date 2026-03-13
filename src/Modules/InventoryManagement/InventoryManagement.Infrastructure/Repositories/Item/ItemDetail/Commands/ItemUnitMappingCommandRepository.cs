using Contracts.Interfaces;
using InventoryManagement.Application.Common.Interfaces;
using InventoryManagement.Application.Item.ItemDetail.Queries.GetAllItems;
using InventoryManagement.Domain.Entities.Item.ItemDetail;
using InventoryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Infrastructure.Repositories.Item.ItemDetail.Commands
{
    public sealed class ItemUnitMappingCommandRepository : ItemLogRepositoryBase, IItemUnitMappingCommandRepository
    {
        public ItemUnitMappingCommandRepository(ApplicationDbContext db, IIPAddressService ipAddressService)
            : base(db, ipAddressService) { }

        public async Task<IReadOnlyList<ItemUnitMapping>> GetByItemIdAsync(int itemId, CancellationToken ct)
        {
            var list = await _db.ItemUnitMapping
                .Where(x => x.ItemId == itemId)
                .ToListAsync(ct);
            return list;
        }

        public async Task CreateAsync(ItemUnitMapping entity, CancellationToken ct)
        {
            await _db.ItemUnitMapping.AddAsync(entity, ct);
            await _db.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(int itemId, IReadOnlyCollection<ItemUnitMappingDto> rows, CancellationToken ct)
        {
            // Normalize incoming payload
            var incoming = (rows ?? Array.Empty<ItemUnitMappingDto>())
                .Where(r => r is not null && r.ProcurementId > 0 && r.ItemGroupId > 0 && r.UnitId > 0)
                .Select(r => new ItemUnitMapping
                {
                    ItemId = itemId,
                    ProcurementId = r.ProcurementId,
                    ItemGroupId = r.ItemGroupId,
                    UnitId = r.UnitId
                })
                .ToList();

            // Load existing tracked rows
            var existing = await _db.ItemUnitMapping
                .Where(x => x.ItemId == itemId)
                .ToListAsync(ct);

            // Build key sets (ProcurementId, ItemGroupId, UnitId)
            var incomingKeys = new HashSet<(int ProcurementId, int ItemGroupId, int UnitId)>(
                incoming.Select(i => (i.ProcurementId, i.ItemGroupId, i.UnitId)));

            var existingByKey = existing.ToDictionary(e => (e.ProcurementId, e.ItemGroupId, e.UnitId));

            // INSERT new rows
            foreach (var inc in incoming)
            {
                if (!existingByKey.ContainsKey((inc.ProcurementId, inc.ItemGroupId, inc.UnitId)))
                {
                    await _db.ItemUnitMapping.AddAsync(inc, ct);
                }
            }

            // DELETE rows not in payload
            foreach (var cur in existing)
            {
                var key = (cur.ProcurementId, cur.ItemGroupId, cur.UnitId);
                if (!incomingKeys.Contains(key))
                {
                    _db.ItemUnitMapping.Remove(cur);
                }
            }
        }
    }
}
