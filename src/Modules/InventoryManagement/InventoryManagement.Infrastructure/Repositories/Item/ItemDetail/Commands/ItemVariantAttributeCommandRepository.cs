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
            if (a.Order <= 0) a.Order = 1;
        }

        // Remove obvious nulls and keep only last per SpecificationMasterId
        var cleaned = attrs
            .Where(a => a is not null && a.SpecificationMasterId > 0)
            .GroupBy(a => a.SpecificationMasterId)
            .Select(g =>
            {
                var row = g.OrderBy(x => x.Order).First();
                return new ItemVariantAttribute
                {
                    Id = row.Id,
                    ItemId = itemId,
                    SpecificationMasterId = row.SpecificationMasterId,
                    Order = row.Order
                };
            })
            .ToList();

        if (cleaned.Count == 0) return;

        // ---------- VALIDATE FK targets exist ----------
        var specMasterIds = cleaned.Select(a => a.SpecificationMasterId)
                            .Distinct()
                            .ToList();

        if (specMasterIds.Count > 0)
        {
            var found = await _db.ItemSpecificationMaster
                .Where(m => specMasterIds.Contains(m.Id) && m.IsDeleted == 0)
                .Select(m => m.Id)
                .ToListAsync(ct);

            var missing = specMasterIds.Except(found).ToList();
            if (missing.Count > 0)
                throw new InvalidOperationException(
                    $"Invalid ItemSpecificationMaster Id(s) in VariantAttributes: {string.Join(", ", missing)}");
        }

        // ---------- UPSERT ----------
        var existing = await _db.Set<ItemVariantAttribute>()
                                .Where(x => x.ItemId == itemId)
                                .ToListAsync(ct);

        var existingById = existing.Where(e => e.Id > 0).ToDictionary(e => e.Id);
        var existingBySpec = existing.ToDictionary(e => e.SpecificationMasterId);

        var keepSpecIds = new HashSet<int>();

        foreach (var inc in cleaned)
        {
            if (inc.Id > 0 && existingById.TryGetValue(inc.Id, out var rowById))
            {
                if (rowById.SpecificationMasterId != inc.SpecificationMasterId &&
                    existingBySpec.TryGetValue(inc.SpecificationMasterId, out var clash) &&
                    clash.Id != rowById.Id)
                {
                    clash.Order = inc.Order;
                    keepSpecIds.Add(clash.SpecificationMasterId);
                }
                else
                {
                    rowById.SpecificationMasterId = inc.SpecificationMasterId;
                    rowById.Order = inc.Order;
                    keepSpecIds.Add(rowById.SpecificationMasterId);
                    existingBySpec[rowById.SpecificationMasterId] = rowById;
                }
                continue;
            }

            if (existingBySpec.TryGetValue(inc.SpecificationMasterId, out var row))
            {
                row.Order = inc.Order;
                keepSpecIds.Add(row.SpecificationMasterId);
            }
            else
            {
                await _db.Set<ItemVariantAttribute>().AddAsync(inc, ct);
                keepSpecIds.Add(inc.SpecificationMasterId);
            }
        }

        bool deleteMissing = false;
        if (deleteMissing)
        {
            var toDelete = existing.Where(e => !keepSpecIds.Contains(e.SpecificationMasterId)).ToList();
            if (toDelete.Count > 0) _db.RemoveRange(toDelete);
        }
    }

    public async Task<List<VariantAttributeDto>> GetForItemAsync(int itemId, CancellationToken ct = default)
    {
        return await _db.Set<ItemVariantAttribute>()
            .AsNoTracking()
            .Where(a => a.ItemId == itemId)
            .OrderBy(a => a.Order)
            .Select(a => new VariantAttributeDto
            {
                Id = a.Id,
                SpecificationMasterId = a.SpecificationMasterId,
                Order = a.Order,
                SpecificationName = a.SpecificationMaster != null
                    ? a.SpecificationMaster.SpecificationName
                    : null
            })
            .ToListAsync(ct);
    }

    public async Task<Dictionary<int, (int SpecMasterId, string? SpecValueName)>> GetSpecificationValueMapAsync(
        IEnumerable<int> specificationValueIds, CancellationToken ct = default)
    {
        var ids = (specificationValueIds ?? Enumerable.Empty<int>())
            .Where(id => id > 0)
            .Distinct()
            .ToList();

        if (ids.Count == 0)
            return new Dictionary<int, (int, string?)>();

        var rows = await _db.ItemSpecificationValue
            .AsNoTracking()
            .Where(v => ids.Contains(v.Id) && v.IsDeleted == InventoryManagement.Domain.Common.BaseEntity.IsDelete.NotDeleted)
            .Select(v => new { v.Id, v.SpecificationMasterId, v.SpecificationValue })
            .ToListAsync(ct);

        return rows.ToDictionary(r => r.Id, r => (r.SpecificationMasterId, (string?)r.SpecificationValue));
    }

    public async Task AddMissingTemplateOptionsAsync(int templateItemId, IEnumerable<VariantValueDto> options, CancellationToken ct = default)
    {
        var rows = (options ?? Enumerable.Empty<VariantValueDto>())
            .Where(v => v?.VariantAttributeId is int id && id > 0 && v.SpecificationValueId > 0)
            .GroupBy(v => new { v!.VariantAttributeId!.Value, v.SpecificationValueId })
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
                x.SpecificationValueId == r.SpecificationValueId, ct);

            if (!exists)
            {
                await _db.Set<ItemVariantValue>().AddAsync(new ItemVariantValue
                {
                    ItemId             = templateItemId,
                    ParentItemId       = templateItemId,
                    VariantAttributeId = r.Value,
                    SpecificationValueId = r.SpecificationValueId
                }, ct);
            }
        }
    }
}
