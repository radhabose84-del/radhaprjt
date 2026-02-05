// ItemManufactureCommandRepository.cs
using InventoryManagement.Application.Common.Interfaces;
using InventoryManagement.Application.Common.Interfaces.Item.ItemDetail.Commands;
using InventoryManagement.Application.Item.ItemDetail.Queries.GetAllItems;
using InventoryManagement.Domain.Entities.Item.ItemDetail;
using InventoryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Infrastructure.Repositories.Item.ItemDetail.Commands
{
    public sealed class ItemManufactureCommandRepository : ItemLogRepositoryBase, IItemManufactureCommandRepository
{
    public ItemManufactureCommandRepository(ApplicationDbContext db, IIPAddressService ipAddressService)
        : base(db, ipAddressService) { }

    public async Task<IReadOnlyList<ItemManufacture>> GetByItemIdAsync(int itemId, CancellationToken ct)
    {
        return await _db.ItemManufacture.Where(x => x.ItemId == itemId).ToListAsync(ct);
    }

    public async Task UpdateAsync(int itemId, IReadOnlyCollection<ItemManufactureDto> rows, CancellationToken ct)
    {
        var doDeleteMissing = true; // set false if you want insert/update only

        var incoming = (rows ?? Array.Empty<ItemManufactureDto>())
            .Where(r => r is not null && r.UnitId > 0 && r.ManufacturingTypeId > 0)
            .Select(r => new ItemManufacture
            {
                ItemId = itemId,
                UnitId = r.UnitId,
                ManufacturingTypeId = r.ManufacturingTypeId
            })
            .ToList();

        // Ensure the ItemMaster exists before proceeding
        var itemMaster = await _db.ItemMaster.FindAsync(itemId);
        if (itemMaster == null)
        {
            throw new Exception("ItemMaster does not exist.");
        }

        var existing = await _db.ItemManufacture.Where(x => x.ItemId == itemId).ToListAsync(ct);
        var existingByKey = existing.ToDictionary(e => (e.UnitId, e.ManufacturingTypeId));
        var incomingKeys = new HashSet<(int UnitId, int TypeId)>(incoming.Select(i => (i.UnitId, i.ManufacturingTypeId)));

        // INSERT (no scalars to update in your model)
        foreach (var inc in incoming)
        {
            if (!existingByKey.ContainsKey((inc.UnitId, inc.ManufacturingTypeId)))
            {
                await _db.ItemManufacture.AddAsync(inc, ct);
            }
        }

        // DELETE missing (optional)
        if (doDeleteMissing)
        {
            foreach (var cur in existing)
            {
                var key = (cur.UnitId, cur.ManufacturingTypeId);
                if (!incomingKeys.Contains(key))
                {
                    _db.ItemManufacture.Remove(cur);
                    _db.ItemLog.Add(new InventoryManagement.Domain.Entities.Item.ItemDetail.ItemLog
                    {
                        EntityName   = nameof(ItemManufacture),
                        EntityId     = itemId,
                        Action       = "Delete",
                        PropertyName = $"Manufacture({cur.UnitId},{cur.ManufacturingTypeId})",
                        OldValue     = null,
                        NewValue     = null
                    });
                }
            }
        }

        // Save is done by UoW
    }
}

}