using InventoryManagement.Application.Common.Interfaces.Item.ItemDetail.Commands;
using InventoryManagement.Application.Item.ItemDetail.Queries.GetAllItems;
using InventoryManagement.Domain.Entities.Item.ItemDetail;
using InventoryManagement.Domain.Entities.Item.ItemDetail.Variant;
using InventoryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public sealed class ItemVariantValueCommandRepository : IItemVariantValueCommandRepository
{
    private readonly ApplicationDbContext _db;
    public ItemVariantValueCommandRepository(ApplicationDbContext db) => _db = db;

    public Task<List<ItemVariantValue>> GetForItemAsync(int itemId, CancellationToken ct = default) =>
        _db.Set<ItemVariantValue>()
           .AsNoTracking()
           .Where(x => x.ItemId == itemId)
           .ToListAsync(ct);

    public async Task UpsertListAsync(int itemId, IEnumerable<VariantValueDto> values, CancellationToken ct = default)
    {
        var incoming = (values ?? Enumerable.Empty<VariantValueDto>())
            .Where(v => v is not null && v.VariantAttributeId.HasValue && v.VariantAttributeId.Value > 0 && v.SpecificationValueId > 0)
            .Select(v => new { VariantAttributeId = v.VariantAttributeId!.Value, v.SpecificationValueId })
            .GroupBy(x => x.VariantAttributeId)
            .Select(g => g.Last())
            .ToList();

        var child = await _db.Set<ItemMaster>()
            .AsNoTracking()
            .Where(i => i.Id == itemId)
            .Select(i => new { i.Id, i.ParentItemId })
            .FirstOrDefaultAsync(ct);

        if (child is null)
            throw new InvalidOperationException($"ItemId={itemId} not found.");

        // Template rows: ParentItemId = itself; Child rows: ParentItemId = its template id
        var templateId = child.ParentItemId ?? child.Id;

        if (incoming.Count == 0)
        {
            var all = await _db.Set<ItemVariantValue>().Where(x => x.ItemId == itemId).ToListAsync(ct);
            if (all.Count > 0) _db.RemoveRange(all);
            return;
        }

        // Validate VariantAttributeIds belong to the template
        var attrIds = incoming.Select(i => i.VariantAttributeId).ToList();
        var validAttrIds = await _db.Set<ItemVariantAttribute>()
            .AsNoTracking()
            .Where(a => a.ItemId == templateId && attrIds.Contains(a.Id))
            .Select(a => a.Id)
            .ToListAsync(ct);

        var missing = attrIds.Except(validAttrIds).ToList();
        if (missing.Count > 0)
            throw new InvalidOperationException($"VariantAttributeId(s) do not belong to template {templateId}: {string.Join(", ", missing)}.");

        // Load existing selections for this child
        var existing = await _db.Set<ItemVariantValue>()
            .Where(x => x.ItemId == itemId)
            .ToListAsync(ct);

        var byAttr = existing.ToDictionary(e => e.VariantAttributeId);

        // Delete removed attributes
        var incomingSet = attrIds.ToHashSet();
        var toDelete = existing.Where(e => !incomingSet.Contains(e.VariantAttributeId)).ToList();
        if (toDelete.Count > 0) _db.RemoveRange(toDelete);

        // Upsert
        foreach (var inc in incoming)
        {
            if (byAttr.TryGetValue(inc.VariantAttributeId, out var row))
            {
                if (row.SpecificationValueId != inc.SpecificationValueId)
                    row.SpecificationValueId = inc.SpecificationValueId;

                // ensure ParentItemId is correct even if older data was missing/wrong
                if (row.ParentItemId != templateId)
                    row.ParentItemId = templateId;
            }
            else
            {
                await _db.Set<ItemVariantValue>().AddAsync(new ItemVariantValue
                {
                    ItemId               = itemId,
                    ParentItemId         = templateId,
                    VariantAttributeId   = inc.VariantAttributeId,
                    SpecificationValueId = inc.SpecificationValueId
                }, ct);
            }
        }
        // Save handled by UoW
    }

    // Back-compat: if your interface still exposes this name, forward to UpsertListAsync.
    public Task UpsertChildSelectionsAsync(int childItemId, IEnumerable<VariantValueDto> values, CancellationToken ct = default)
        => UpsertListAsync(childItemId, values, ct);

    // Back-compat stubs – old schema functions no longer apply (template no longer stores values).
    public Task MapOptionToChildAsync(int templateItemId, int attributeId, string optionValue, int childItemId, CancellationToken ct = default)
        => Task.CompletedTask;

    public Task AddMissingTemplateOptionsAsync(int templateItemId, IEnumerable<VariantValueDto> options, CancellationToken ct = default)
        => Task.CompletedTask;
}
