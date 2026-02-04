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

        // Helper: normalize option for keying
        private static string N(string s) => (s ?? string.Empty).Trim().ToLowerInvariant();

        // Helper: build a stable combo key "AttributeId:option|AttributeId:option" ordered by AttributeId
        private static string BuildComboKey(IEnumerable<(int AttributeId, string Option)> parts)
        {
            return string.Join("|", parts
                .OrderBy(p => p.AttributeId)
                .Select(p => $"{p.AttributeId}:{N(p.Option)}"));
        }

        public async Task<Dictionary<int, List<string>>> GetForItemGroupedAsync(int itemId, CancellationToken ct = default)
        {
            var rows = await _db.Set<ItemVariantValue>()
                .AsNoTracking()
                .Where(v => v.ItemId == itemId)
                .Select(v => new
                {
                    v.OptionValue,
                    AttributeId = v.VariantAttribute.AttributeId
                })
                .ToListAsync(ct);

            return rows
                .GroupBy(r => r.AttributeId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => x.OptionValue)
                          .Distinct(System.StringComparer.OrdinalIgnoreCase)
                          .ToList());
        }

     // IItemVariantValueQueryRepository impl snippet
        public async Task<HashSet<string>> GetExistingChildComboKeysAsync(int templateItemId, CancellationToken ct = default)
        {
            // Build map of VariantAttributeId that belong to this template
            var varAttrIds = await _db.ItemVariantAttribute
                .Where(a => a.ItemId == templateItemId)
                .Select(a => a.Id)
                .ToListAsync(ct);

            // Group child values by child item, create a key "varAttrId:value|varAttrId:value|..."
            var keys = await _db.ItemVariantValue
                .Where(v => v.ItemMaster.ParentItemId == templateItemId && varAttrIds.Contains(v.VariantAttributeId))
                .GroupBy(v => v.ItemId)
                .Select(g => string.Join("|",
                    g.OrderBy(x => x.VariantAttributeId)
                    .Select(x => x.VariantAttributeId.ToString() + ":" + x.OptionValue.Trim().ToLower())))
                .ToListAsync(ct);

            return new HashSet<string>(keys, StringComparer.Ordinal);
        }

        public async Task<Dictionary<string, int>> GetExistingChildCombosWithIdsAsync(int templateItemId, CancellationToken ct = default)
        {
            // 1) Attribute definitions for the template (we’ll use AttributeId for key ordering)
            var attrDefs = await _db.Set<ItemVariantAttribute>()
                .AsNoTracking()
                .Where(a => a.ItemId == templateItemId)
                .Select(a => new { a.Id, a.AttributeId, a.Order })
                .OrderBy(a => a.Order)
                .ToListAsync(ct);

            if (attrDefs.Count == 0)
                return new Dictionary<string, int>(0);

            var attrIds = attrDefs.Select(a => a.AttributeId).ToList();

            // 2) Child items under the template
            var childIds = await _db.ItemMaster
                .Where(i => i.ParentItemId == templateItemId &&
                            i.IsDeleted == BaseEntity.IsDelete.NotDeleted)
                .Select(i => i.Id)
                .ToListAsync(ct);

            if (childIds.Count == 0)
                return new Dictionary<string, int>(0);

            // 3) Values for those children (need AttributeId via VariantAttribute)
            var vals = await _db.Set<ItemVariantValue>()
                .AsNoTracking()
                .Where(v => childIds.Contains(v.ItemId))
                .Select(v => new
                {
                    v.ItemId,
                    v.OptionValue,
                    AttributeId = v.VariantAttribute.AttributeId
                })
                .ToListAsync(ct);

            // 4) Build keys per child (only if we have a value for every attribute)
            var byChild = vals.GroupBy(v => v.ItemId);
            var result = new Dictionary<string, int>(StringComparer.Ordinal);

            foreach (var g in byChild)
            {
                var parts = g
                    .Where(x => attrIds.Contains(x.AttributeId))
                    .Select(x => (x.AttributeId, x.OptionValue))
                    .ToList();

                if (parts.Count != attrIds.Count)
                    continue; // skip partial combos

                var key = BuildComboKey(parts);
                if (!result.ContainsKey(key))
                    result[key] = g.Key; // first wins; duplicates shouldn’t happen
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
                    VariantAttributeId = v.VariantAttributeId,                    
                    OptionValue        = v.OptionValue,
                    Combo              = null
                })
                .ToListAsync(ct);
        }
    }
}
