using InventoryManagement.Application.Common.Interfaces.Item.ItemDetail.Commands;
using InventoryManagement.Application.Item.ItemDetail.Queries.GetAllItems;
using InventoryManagement.Domain.Entities.Item.ItemDetail.Variant;
using InventoryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public sealed class ItemVariantAttributeCommandRepository : IItemVariantAttributeCommandRepository
{
    private readonly ApplicationDbContext _db;
    public ItemVariantAttributeCommandRepository(ApplicationDbContext db) => _db = db;

    public async Task UpsertAttributesAsync(
        int itemId,
        List<VariantAttributeDto> attrs,
        CancellationToken ct = default)
    {
        attrs ??= new();        
        // ---------- Normalize payload ----------
        foreach (var a in attrs)
        {
            if (a is null) continue;          

            // Order must be >= 1, keep 0+ as-is if you want to allow it.
            if (a.Order <= 0) a.Order = 1;
        }

        // Remove obvious nulls and keep only last per AttributeId (or choose your rule)
        var cleaned = attrs
            .Where(a => a is not null && a.AttributeId > 0)
            .GroupBy(a => a.AttributeId)
            .Select(g =>
            {
                // pick the one with the smallest Order, then the latest (or however you want)
                var row = g.OrderBy(x => x.Order).First();
                return new ItemVariantAttribute
                {
                    Id = row.Id,
                    ItemId = itemId,
                    AttributeId = row.AttributeId,
                    VariantBasedOn = row.VariantBasedOn,
                    AttributeGroupId = row.AttributeGroupId,
                    Order = row.Order
                };
            })
            .ToList();

        // If nothing left, nothing to do (and we won't delete existing here).
        if (cleaned.Count == 0) return;

        // ---------- VALIDATE FK targets exist ----------
        // Validate AttributeId + VariantBasedOn against MiscMaster
        var miscIds = cleaned.Select(a => a.AttributeId)
                            .Concat(cleaned.Select(a => a.VariantBasedOn))
                            .Where(id => id > 0)
                            .Distinct()
                            .ToList();

        if (miscIds.Count > 0)
        {
            var found = await _db.MiscMaster
                .Where(m => miscIds.Contains(m.Id) && m.IsDeleted == 0)
                .Select(m => m.Id)
                .ToListAsync(ct);

            var missing = miscIds.Except(found).ToList();
            if (missing.Count > 0)
                throw new InvalidOperationException(
                    $"Invalid MiscMaster Id(s) in VariantAttributes: {string.Join(", ", missing)}");
        }

        // Validate AttributeGroupId in MiscTypeMaster (when provided)
        var groupIds = cleaned.Where(a => a.AttributeGroupId.HasValue)
                            .Select(a => a.AttributeGroupId!.Value)
                            .Distinct()
                            .ToList();

        if (groupIds.Count > 0)
        {
            var groupsFound = await _db.MiscTypeMaster
                .Where(t => groupIds.Contains(t.Id) && t.IsDeleted == 0)
                .Select(t => t.Id)
                .ToListAsync(ct);

            var groupsMissing = groupIds.Except(groupsFound).ToList();
            if (groupsMissing.Count > 0)
                throw new InvalidOperationException(
                    $"Invalid AttributeGroupId(s): {string.Join(", ", groupsMissing)}");
        }

        // ---------- UPSERT ----------
        var existing = await _db.Set<ItemVariantAttribute>()
                                .Where(x => x.ItemId == itemId)
                                .ToListAsync(ct);

        var existingById = existing.Where(e => e.Id > 0).ToDictionary(e => e.Id);
        var existingByAttr = existing.ToDictionary(e => e.AttributeId); // (ItemId, AttributeId) should be unique

        var keepAttrIds = new HashSet<int>();

        foreach (var inc in cleaned)
        {
            // If payload has a concrete Id that matches an existing row: update that row.
            if (inc.Id > 0 && existingById.TryGetValue(inc.Id, out var rowById))
            {
                // If AttributeId changed and clashes with another row, either:
                //   a) throw, or b) merge. Here we "merge" by moving to that AttributeId if not already present.
                if (rowById.AttributeId != inc.AttributeId &&
                    existingByAttr.TryGetValue(inc.AttributeId, out var clash) &&
                    clash.Id != rowById.Id)
                {
                    // Resolve clash — simplest is to update the clashing row instead and keep current as is.
                    // Here we prefer update the "clash" and mark it kept; skip changing rowById.AttributeId.
                    clash.VariantBasedOn = inc.VariantBasedOn;
                    clash.AttributeGroupId = inc.AttributeGroupId;
                    clash.Order = inc.Order;
                    keepAttrIds.Add(clash.AttributeId);
                }
                else
                {
                    rowById.AttributeId = inc.AttributeId;
                    rowById.VariantBasedOn = inc.VariantBasedOn;
                    rowById.AttributeGroupId = inc.AttributeGroupId;
                    rowById.Order = inc.Order;
                    keepAttrIds.Add(rowById.AttributeId);

                    // keep dictionaries coherent if AttributeId changed
                    existingByAttr[rowById.AttributeId] = rowById;
                }

                continue;
            }

            // Otherwise, match by AttributeId (unique per item)
            if (existingByAttr.TryGetValue(inc.AttributeId, out var row))
            {
                row.VariantBasedOn = inc.VariantBasedOn;
                row.AttributeGroupId = inc.AttributeGroupId;
                row.Order = inc.Order;
                keepAttrIds.Add(row.AttributeId);
            }
            else
            {
                await _db.Set<ItemVariantAttribute>().AddAsync(inc, ct);
                keepAttrIds.Add(inc.AttributeId);
            }
        }

        // ---------- OPTIONAL: delete attributes not in payload ----------
        // WARNING: Deleting attributes that are referenced by template values or existing child variants
        // will violate FKs. Enable only if you guarantee no dependencies, or handle cascading clean-up.
        bool deleteMissing = false; // set true only if safe
        if (deleteMissing)
        {
            var toDelete = existing.Where(e => !keepAttrIds.Contains(e.AttributeId)).ToList();
            if (toDelete.Count > 0) _db.RemoveRange(toDelete);
        }

        // NOTE: do not call SaveChanges here if you’re wrapped in a UnitOfWork;
        // let the outer transaction commit.
    }
    public async Task<List<VariantAttributeDto>> GetForItemAsync(int itemId, CancellationToken ct = default)
    {
        return await _db.Set<ItemVariantAttribute>()
            .AsNoTracking()
            .Where(a => a.ItemId == itemId)
            .OrderBy(a => a.Order)
            .Select(a => new VariantAttributeDto
            {
                Id = a.Id,                // << MUST be the PK of ItemVariantAttribute
                AttributeId = a.AttributeId,
                VariantBasedOn = a.VariantBasedOn,
                AttributeGroupId = a.AttributeGroupId,
                Order = a.Order,
                AttributeName   = a.MiscAttribute != null ? a.MiscAttribute.Code : null
            })
            .ToListAsync(ct);
    }

    public async Task AddMissingTemplateOptionsAsync(int templateItemId, IEnumerable<VariantValueDto> options, CancellationToken ct = default)
    {
        var rows = (options ?? Enumerable.Empty<VariantValueDto>())
            .Where(v => v?.VariantAttributeId is int id && id > 0 && !string.IsNullOrWhiteSpace(v!.OptionValue))
            .GroupBy(v => new { v!.VariantAttributeId!.Value, Opt = v.OptionValue.Trim() })
            .Select(g => g.Key)
            .ToList();

        if (rows.Count == 0) return;

        var attrIds = rows.Select(r => r.Value).ToList();
        var ok = await _db.Set<ItemVariantAttribute>()
            .AsNoTracking()
            .Where(a => a.ItemId == templateItemId && attrIds.Contains(a.Id))
            .Select(a => a.Id)
            .ToListAsync(ct);

        foreach (var r in rows)
        {
            if (!ok.Contains(r.Value)) continue;
            var exists = await _db.Set<ItemVariantValue>().AnyAsync(x =>
                x.ItemId == templateItemId &&
                x.VariantAttributeId == r.Value &&
                x.OptionValue == r.Opt, ct);

            if (!exists)
            {
                await _db.Set<ItemVariantValue>().AddAsync(new ItemVariantValue
                {
                    ItemId             = templateItemId,
                    ParentItemId       = templateItemId,        // ← NEW
                    VariantAttributeId = r.Value,
                    OptionValue        = r.Opt
                }, ct);
            }
        }
    }
    
}
