using Contracts.Interfaces;
using InventoryManagement.Application.Common.Interfaces;
using InventoryManagement.Application.Item.ItemDetail.Queries.GetAllItems;
using InventoryManagement.Domain.Entities.Item.ItemDetail;
using InventoryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Infrastructure.Repositories.Item.ItemDetail.Commands
{
    public sealed class ItemSupplierCommandRepository : ItemLogRepositoryBase, IItemSupplierCommandRepository
    {       
        public ItemSupplierCommandRepository(ApplicationDbContext db, IIPAddressService ipAddressService)
            : base(db, ipAddressService) { }
        public async Task<IReadOnlyList<ItemSupplier>> GetByItemIdAsync(int itemId, CancellationToken ct)
        {
            var list = await _db.ItemSupplier
                .Where(x => x.ItemId == itemId)
                .ToListAsync(ct);
            return list;
        }

        /// <summary>
        /// Synchronize suppliers for an item:
        ///  - Insert new (SupplierId, UnitId) pairs
        ///  - Update changed scalar fields (e.g., SupplierPartNo)
        ///  - Delete rows that are not in payload  (toggle 'doDeleteMissing' if you want insert/update only)
        /// </summary>
        public async Task UpdateAsync(int itemId, IReadOnlyCollection<ItemSupplierDto> rows, CancellationToken ct)
        {
            var doDeleteMissing = true; // set false if you want insert/update only

            // Normalize incoming payload
            var incoming = (rows ?? Array.Empty<ItemSupplierDto>())
                .Where(r => r is not null && r.SupplierId > 0 && r.UnitId > 0)
                .Select(r => new ItemSupplier
                {
                    ItemId = itemId,
                    SupplierId = r.SupplierId,
                    UnitId = r.UnitId,
                    SupplierPartNo = string.IsNullOrWhiteSpace(r.SupplierPartNo) ? null : r.SupplierPartNo.Trim(),
                    LeadTime = r.LeadTime,
                    MOQ = r.MOQ,
                    MOQUomId = r.MOQUomId,
                    PackageValue = r.PackageValue,
                    PackageUomId = r.PackageUomId,
                    DefaultSupplier=r.DefaultSupplier
                })
                .ToList();

            // Load existing tracked rows
            var existing = await _db.ItemSupplier
                .Where(x => x.ItemId == itemId)
                .ToListAsync(ct);

            // Build key sets (SupplierId, UnitId)
            var incomingKeys = new HashSet<(int SupplierId, int UnitId)>(
                incoming.Select(i => (i.SupplierId, i.UnitId)));

            var existingByKey = existing.ToDictionary(e => (e.SupplierId, e.UnitId));

            // INSERT or UPDATE
            foreach (var inc in incoming)
            {
                if (existingByKey.TryGetValue((inc.SupplierId, inc.UnitId), out var cur))
                {
                    // UPDATE scalar fields (compare first; only set if changed to keep logs clean)
                    var changes = new List<PropertyChange>();

                    if (!string.Equals(cur.SupplierPartNo, inc.SupplierPartNo, StringComparison.Ordinal))
                    {
                        changes.Add(new PropertyChange(
                            Property: $"Supplier({cur.SupplierId},{cur.UnitId}).SupplierPartNo",
                            OldValue: cur.SupplierPartNo,
                            NewValue: inc.SupplierPartNo
                        ));
                        cur.SupplierPartNo = inc.SupplierPartNo;
                    }
                    changes.Add(new PropertyChange(
                        $"Supplier({cur.SupplierId},{cur.UnitId}).LeadTime",
                        cur.LeadTime, 
                        inc.LeadTime  
                    ));
                    changes.Add(new PropertyChange(
                        $"Supplier({cur.SupplierId},{cur.UnitId}).MOQ",
                        cur.MOQ,    
                        inc.MOQ     
                    ));
                    changes.Add(new PropertyChange(
                        $"Supplier({cur.SupplierId},{cur.UnitId}).MOQUomId",
                        cur.MOQUomId,    
                        inc.MOQUomId     
                    ));
                    changes.Add(new PropertyChange(
                        $"Supplier({cur.SupplierId},{cur.UnitId}).PackageValue",
                        cur.PackageValue,    
                        inc.PackageValue     
                    ));
                    changes.Add(new PropertyChange(
                        $"Supplier({cur.SupplierId},{cur.UnitId}).PackageUomId",
                        cur.PackageUomId,    
                        inc.PackageUomId     
                    ));
                    changes.Add(new PropertyChange(
                        $"Supplier({cur.SupplierId},{cur.UnitId}).DefaultSupplier",
                        cur.DefaultSupplier, 
                        inc.DefaultSupplier  
                    ));
                    if (changes.Count > 0)
                        TryAddUpdateLog(nameof(ItemSupplier), itemId, changes);
                }
                else
                {
                    // INSERT
                    await _db.ItemSupplier.AddAsync(inc, ct);
                   // AddInsertLog(nameof(ItemSupplier), itemId);
                }
            }

            // DELETE missing (optional)
            if (doDeleteMissing)
            {
                foreach (var cur in existing)
                {
                    var key = (cur.SupplierId, cur.UnitId);
                    if (!incomingKeys.Contains(key))
                    {
                        _db.ItemSupplier.Remove(cur);

                        // log delete as an update entry with a marker, or create a separate ItemLog Action="Delete"
                        _db.ItemLog.Add(new InventoryManagement.Domain.Entities.Item.ItemDetail.ItemLog
                        {
                            EntityName   = nameof(ItemSupplier),
                            EntityId     = itemId,
                            Action       = "Delete",
                            PropertyName = $"Supplier({cur.SupplierId},{cur.UnitId})",
                            OldValue     = cur.SupplierPartNo,
                            NewValue     = null
                        });
                    }
                }
            }
        }
    }
}
