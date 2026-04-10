using InventoryManagement.Application.Common.Interfaces.Item.ItemDetail.Queries;
using InventoryManagement.Application.Item.ItemDetail.Queries.GetAllItems;
using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Entities.Item.ItemDetail.Variant;
using InventoryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryManagement.Infrastructure.Repositories.Item.ItemDetail.Queries
{
    public sealed class ItemVariantValueQueryRepository : IItemVariantValueQueryRepository
    {
        private readonly ApplicationDbContext _db;
        public ItemVariantValueQueryRepository(ApplicationDbContext db) => _db = db;

        // Helper: build a stable combo key "varAttrId:specValueId|varAttrId:specValueId" ordered by VarAttrId
        private static string BuildComboKey(IEnumerable<(int VarAttrId, int SpecValueId)> parts)
        {
            return string.Join("|", parts
                .OrderBy(p => p.VarAttrId)
                .Select(p => $"{p.VarAttrId}:{p.SpecValueId}"));
        }

        public async Task<Dictionary<int, List<int>>> GetForItemGroupedAsync(int itemId, CancellationToken ct = default)
        {
            var rows = await _db.Set<ItemVariantValue>()
                .AsNoTracking()
                .Where(v => v.ItemId == itemId)
                .Select(v => new
                {
                    v.SpecificationValueId,
                    v.VariantAttributeId
                })
                .ToListAsync(ct);

            return rows
                .GroupBy(r => r.VariantAttributeId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => x.SpecificationValueId)
                          .Distinct()
                          .ToList());
        }

        public async Task<HashSet<string>> GetExistingChildComboKeysAsync(int templateItemId, CancellationToken ct = default)
        {
            // Build map of VariantAttributeId that belong to this template
            var varAttrIds = await _db.ItemVariantAttribute
                .Where(a => a.ItemId == templateItemId)
                .Select(a => a.Id)
                .ToListAsync(ct);

            // Group child values by child item, create a key "varAttrId:specValueId|varAttrId:specValueId|..."
            var keys = await _db.ItemVariantValue
                .Where(v => v.ItemMaster!.ParentItemId == templateItemId && varAttrIds.Contains(v.VariantAttributeId))
                .GroupBy(v => v.ItemId)
                .Select(g => string.Join("|",
                    g.OrderBy(x => x.VariantAttributeId)
                    .Select(x => x.VariantAttributeId.ToString() + ":" + x.SpecificationValueId.ToString())))
                .ToListAsync(ct);

            return new HashSet<string>(keys, StringComparer.Ordinal);
        }

        public async Task<Dictionary<string, int>> GetExistingChildCombosWithIdsAsync(int templateItemId, CancellationToken ct = default)
        {
            // 1) Attribute definitions for the template
            var attrDefs = await _db.Set<ItemVariantAttribute>()
                .AsNoTracking()
                .Where(a => a.ItemId == templateItemId)
                .Select(a => new { a.Id, a.SpecificationMasterId, a.Order })
                .OrderBy(a => a.Order)
                .ToListAsync(ct);

            if (attrDefs.Count == 0)
                return new Dictionary<string, int>(0);

            var attrRowIds = attrDefs.Select(a => a.Id).ToList();

            // 2) Child items under the template
            var childIds = await _db.ItemMaster
                .Where(i => i.ParentItemId == templateItemId &&
                            i.IsDeleted == BaseEntity.IsDelete.NotDeleted)
                .Select(i => i.Id)
                .ToListAsync(ct);

            if (childIds.Count == 0)
                return new Dictionary<string, int>(0);

            // 3) Values for those children
            var vals = await _db.Set<ItemVariantValue>()
                .AsNoTracking()
                .Where(v => childIds.Contains(v.ItemId))
                .Select(v => new
                {
                    v.ItemId,
                    v.SpecificationValueId,
                    v.VariantAttributeId
                })
                .ToListAsync(ct);

            // 4) Build keys per child (only if we have a value for every attribute)
            var byChild = vals.GroupBy(v => v.ItemId);
            var result = new Dictionary<string, int>(StringComparer.Ordinal);

            foreach (var g in byChild)
            {
                var parts = g
                    .Where(x => attrRowIds.Contains(x.VariantAttributeId))
                    .Select(x => (x.VariantAttributeId, x.SpecificationValueId))
                    .ToList();

                if (parts.Count != attrRowIds.Count)
                    continue; // skip partial combos

                var key = BuildComboKey(parts);
                if (!result.ContainsKey(key))
                    result[key] = g.Key; // first wins
            }

            return result;
        }

        public async Task<List<VariantValueDto>> GetForItemAsync(int itemId, CancellationToken ct = default)
        {
            return await _db.Set<ItemVariantValue>()
                .AsNoTracking()
                .Where(v => v.ItemId == itemId)
                .Select(v => new VariantValueDto
                {
                    VariantAttributeId   = v.VariantAttributeId,
                    SpecificationValueId = v.SpecificationValueId,
                    SpecificationValue   = v.SpecificationValue != null ? v.SpecificationValue.SpecificationValue : null,
                    Combo                = null
                })
                .ToListAsync(ct);
        }
    }
}
