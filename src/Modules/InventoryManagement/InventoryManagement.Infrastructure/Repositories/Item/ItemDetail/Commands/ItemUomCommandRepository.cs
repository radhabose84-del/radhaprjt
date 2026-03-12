// ItemUomCommandRepository.cs
using Contracts.Interfaces;
using InventoryManagement.Application.Common.Interfaces;
using InventoryManagement.Application.Common.Interfaces.Item.ItemDetail.Commands;
using InventoryManagement.Application.Item.ItemDetail.Queries.GetAllItems;
using InventoryManagement.Domain.Entities.Item.ItemDetail;
using InventoryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Infrastructure.Repositories.Item.ItemDetail.Commands
{
    public sealed class ItemUomCommandRepository : ItemLogRepositoryBase, IItemUomCommandRepository
    {       
        public ItemUomCommandRepository(ApplicationDbContext db, IIPAddressService ipAddressService)
            : base(db, ipAddressService) { }
        public async Task<IReadOnlyList<ItemUOM>> GetByItemIdAsync(int itemId, CancellationToken ct)
        {
            // If your DbSet is singular, use _db.ItemUOM
            return await _db.ItemUOMs
                .Where(x => x.ItemId == itemId)
                .ToListAsync(ct);
        }

        public async Task UpdateAsync(int itemId, IReadOnlyCollection<ItemUomDto> rows, CancellationToken ct)
        {
            var doDeleteMissing = true; // set to false if you only want insert/update

            // Normalize incoming and keep only rows with a non-null, positive ConversionUOMId
            var incoming = (rows ?? Array.Empty<ItemUomDto>())
                .Where(r => r is not null && r.ConversionUOMId.HasValue && r.ConversionUOMId.Value > 0)
                .Select(r => new ItemUOM
                {
                    ItemId          = itemId,
                    ConversionUOMId = r.ConversionUOMId!.Value,   // safe due to filter above
                    ConversionRate  = r.ConversionRate
                })
                .ToList();

            // Load existing rows for this item
            var existing = await _db.ItemUOMs
                .Where(x => x.ItemId == itemId)
                .ToListAsync(ct);

            // Build a dictionary keyed by non-null ConversionUOMId
            var existingWithKey = existing
                .Where(e => e.ConversionUOMId.HasValue)
                .ToList();

            var existingByKey = existingWithKey
                .ToDictionary(e => e.ConversionUOMId!.Value);

            // Build the incoming key set (non-null ints)
            var incomingKeys = new HashSet<int>(incoming.Select(i => i.ConversionUOMId!.Value));

            // INSERT or UPDATE
            foreach (var inc in incoming)
            {
                var key = inc.ConversionUOMId!.Value;

                if (existingByKey.TryGetValue(key, out var cur))
                {
                    var changes = new List<PropertyChange>();

                    // Update scalar fields if changed (keeps logs clean)
                    if (cur.ConversionRate != inc.ConversionRate)
                    {
                        changes.Add(new PropertyChange(
                            Property: $"UOM({key}).ConversionRate",
                            OldValue: cur.ConversionRate?.ToString(),
                            NewValue: inc.ConversionRate?.ToString()
                        ));
                        cur.ConversionRate = inc.ConversionRate;
                    }

                    if (changes.Count > 0)
                        TryAddUpdateLog(nameof(ItemUOM), itemId, changes);
                }
                else
                {
                    await _db.ItemUOMs.AddAsync(inc, ct);
                  //  AddInsertLog(nameof(ItemUOM), itemId);
                }
            }

            // DELETE missing (optional)
            if (doDeleteMissing)
            {
                foreach (var cur in existingWithKey)
                {
                    var key = cur.ConversionUOMId!.Value;
                    if (!incomingKeys.Contains(key))
                    {
                        _db.ItemUOMs.Remove(cur);

                        // Log as a delete action
                        _db.ItemLog.Add(new InventoryManagement.Domain.Entities.Item.ItemDetail.ItemLog
                        {
                            EntityName   = nameof(ItemUOM),
                            EntityId     = itemId,
                            Action       = "Delete",
                            PropertyName = $"UOM({key})",
                            OldValue     = cur.ConversionRate?.ToString(),
                            NewValue     = null
                        });
                    }
                }
            }            
        }
    }
}
