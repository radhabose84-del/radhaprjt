#nullable disable
using System.Text;
using AutoMapper;
using InventoryManagement.Application.Common.Interfaces.Item.ItemDetail.Commands;
using InventoryManagement.Application.Common.Interfaces.Item.ItemDetail.Queries;
using InventoryManagement.Application.Item.ItemDetail.Commands.CreateItem;
using InventoryManagement.Application.Item.ItemDetail.Queries.GetAllItems;
using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Entities.Item.ItemDetail;
using InventoryManagement.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace InventoryManagement.Application.Item.ItemAggregate.Handlers
{
    public sealed class CreateItemCommandHandler : IRequestHandler<CreateItemCommand, int>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly ILogger<CreateItemCommandHandler> _logger;

        private readonly IItemCommandRepository _itemRepo;
        private readonly IItemPurchaseCommandRepository _purchaseRepo;
        private readonly IItemInventoryCommandRepository _inventoryRepo;
        private readonly IItemQualityCommandRepository _qualityRepo;
        private readonly IItemSaleCommandRepository _saleRepo;
        private readonly IItemSupplierCommandRepository _supplierRepo;
        private readonly IItemManufactureCommandRepository _manufactureRepo;
        private readonly IItemUomCommandRepository _uomRepo;
        private readonly IItemQueryRepository _itemQry;
        private readonly IItemVariantValueCommandRepository _variantValCmd;
        private readonly IItemVariantValueQueryRepository _variantValQry;
        private readonly IItemVariantAttributeCommandRepository _variantAttrCmd;

        public CreateItemCommandHandler(
            IUnitOfWork uow,
            IMapper mapper,
            IMediator mediator,
            ILogger<CreateItemCommandHandler> logger,
            IItemCommandRepository itemRepo,
            IItemPurchaseCommandRepository purchaseRepo,
            IItemInventoryCommandRepository inventoryRepo,
            IItemQualityCommandRepository qualityRepo,
            IItemSaleCommandRepository saleRepo,
            IItemSupplierCommandRepository supplierRepo,
            IItemManufactureCommandRepository manufactureRepo,
            IItemUomCommandRepository uomRepo,
            IItemQueryRepository itemQry,
            IItemVariantValueCommandRepository variantValCmd,
            IItemVariantValueQueryRepository variantValQry,
            IItemVariantAttributeCommandRepository variantAttrCmd)
        {
            _uow = uow;
            _mapper = mapper;
            _mediator = mediator;
            _logger = logger;

            _itemRepo = itemRepo;
            _purchaseRepo = purchaseRepo;
            _inventoryRepo = inventoryRepo;
            _qualityRepo = qualityRepo;
            _saleRepo = saleRepo;
            _supplierRepo = supplierRepo;
            _manufactureRepo = manufactureRepo;
            _uomRepo = uomRepo;
            _itemQry = itemQry;
            _variantValCmd = variantValCmd;
            _variantValQry = variantValQry;
            _variantAttrCmd = variantAttrCmd;
        }

        public async Task<int> Handle(CreateItemCommand request, CancellationToken ct)
        {
            var p = request.Payload;

            if (p.ParentItemId.HasValue && p.ParentItemId > 0 && p.ParentItemId != null)
                return await CreateVariantItemsAsync(p, ct); 

            if (p.HasVariants)
                return await CreateTemplateAsync(p, ct);     

            return await CreateSingleItemAsync(p, ct);       
        }
        
        private async Task<int> CreateTemplateAsync(ItemDto p, CancellationToken ct)
        {
            if (!p.ItemGroupId.HasValue || !p.ItemCategoryId.HasValue)
                throw new InvalidOperationException("ItemGroupId and ItemCategoryId are required for template creation.");

            var itemCode = await _itemQry.GetLatestItemCode(p.ItemGroupId.Value, p.ItemCategoryId.Value, ct)
                         ?? throw new InvalidOperationException("Failed to generate ItemCode.");

            if (await _itemRepo.ExistsByCodeForCreateAsync(itemCode, ct))
                throw new InvalidOperationException($"Generated ItemCode '{itemCode}' already exists.");

            var templateId = await _uow.ExecuteInTransactionAsync<int>(async _ =>
            {
                // base template
                var item = _mapper.Map<ItemMaster>(p);
                item.ItemCode = itemCode;
                item.HasVariants = true;
                item.ParentItemId = null;
                item.IsActive = BaseEntity.Status.Active;

                var newId = await _itemRepo.CreateAsync(item, ct);

                // tabs (optional)
                if (!DtoEmptyChecker.IsEmpty(p.Purchase))
                {
                    var e = _mapper.Map<ItemPurchase>(p.Purchase);
                    e.ItemId = newId;
                    await _purchaseRepo.CreateAsync(e, ct);
                }

                // Inventory
                if (!DtoEmptyChecker.IsEmpty(p.Inventory))
                {
                    var e = _mapper.Map<ItemInventory>(p.Inventory);
                    e.ItemId = newId;
                    await _inventoryRepo.CreateAsync(e, ct);
                }

                // Quality
                if (!DtoEmptyChecker.IsEmpty(p.Quality))
                {
                    var e = _mapper.Map<ItemQuality>(p.Quality);
                    e.ItemId = newId;
                    await _qualityRepo.CreateAsync(e, ct);
                }

                // Sale
                if (!DtoEmptyChecker.IsEmpty(p.Sale))
                {
                    var e = _mapper.Map<ItemSale>(p.Sale);
                    e.ItemId = newId;
                    await _saleRepo.CreateAsync(e, ct);
                }
                // Collections (null-safe)
                if (DtoEmptyChecker.HasAny(p.Suppliers))   await _supplierRepo.UpdateAsync(newId, p.Suppliers, ct);
                if (DtoEmptyChecker.HasAny(p.Manufacture)) await _manufactureRepo.UpdateAsync(newId, p.Manufacture, ct);
                if (DtoEmptyChecker.HasAny(p.Uoms))        await _uomRepo.UpdateAsync(newId, p.Uoms, ct);
                // ATTRIBUTES ONLY (no values on template)
                if (p.VariantAttributes is { Count: > 0 })
                    await _variantAttrCmd.UpsertAttributesAsync(newId, p.VariantAttributes, ct);

                return newId;
            }, ct);

            await _mediator.Publish(new AuditLogsDomainEvent(
                "CreateTemplate",
                templateId.ToString(),
                p.ItemName,
                "Variant template created (attributes only).",
                "ItemMaster"), ct);

            if (!string.IsNullOrWhiteSpace(p.ItemImage))
                await TryMoveImageAsync(templateId, p.ItemImage!, itemCode, ct);

            return templateId;
        }

        // ---------------- VARIANTS FROM TEMPLATE (ParentItemId set) ----------------
       private async Task<int> CreateVariantItemsAsync(ItemDto p, CancellationToken ct)
        {
            if (p.VariantValues is null || p.VariantValues.Count == 0)
                throw new InvalidOperationException("VariantValues are required to create child items.");

            // 1) Load template
            var template = await _itemRepo.GetTrackingAsync(p.ParentItemId!.Value, ct)
                            ?? throw new InvalidOperationException("Template item not found.");
            if (!template.HasVariants)
                throw new InvalidOperationException("Parent item is not a template.");

            // 2) Load template attributes (Id == VariantAttributeId)
            var attrs = await _variantAttrCmd.GetForItemAsync(template.Id, ct);
            if (attrs.Count == 0)
                throw new InvalidOperationException("Template has no variant attributes defined.");

            var validVarAttrIds = new HashSet<int>(attrs.Select(a => a.Id));
            var orderedAttrIds  = attrs.OrderBy(a => a.Order).Select(a => a.Id).ToList();

            // Maps to support payloads that send AttributeId instead of VariantAttributeId:
            var byVarAttrId = attrs.ToDictionary(a => a.Id);                      // e.g. 31 -> row
            var byAttrId    = attrs.ToDictionary(a => a.AttributeId, a => a.Id);  // e.g. 49 -> 31, 50 -> 32

            // ---- NORMALIZE INCOMING to VariantAttributeId row ids ----
            var normalized = (p.VariantValues ?? new List<VariantValueDto>())
                .Where(v => v != null && !string.IsNullOrWhiteSpace(v.OptionValue))
                .Select(v =>
                {
                    int? id = v.VariantAttributeId;

                    // If the id isn't a known row id, try to interpret it as AttributeId and map to row id.
                    if (!id.HasValue || !byVarAttrId.ContainsKey(id.Value))
                    {
                        if (id.HasValue && byAttrId.TryGetValue(id.Value, out var mapped))
                            id = mapped;        // AttributeId -> VariantAttribute row id
                        else
                            id = null;          // still invalid, drop later
                    }

                    return new VariantValueDto
                    {
                        VariantAttributeId = id,
                        OptionValue = v.OptionValue.Trim(),
                        Combo = v.Combo
                    };
                })
                .Where(v => v.VariantAttributeId.HasValue && validVarAttrIds.Contains(v.VariantAttributeId.Value))
                .ToList();

            if (normalized.Count == 0)
                throw new InvalidOperationException(
                    "No valid variant selections were provided. " +
                    "Each selection must reference a VariantAttribute row id, or an AttributeId that belongs to the template.");

            // 3) Existing combos from real children (protect against duplicates/overlaps)
            var existingCombos = new List<HashSet<(int VarAttrId, string Opt)>>();
            var childIds = await _itemRepo.GetChildIdsAsync(template.Id, ct);
            foreach (var cid in childIds)
            {
                var rows = await _variantValQry.GetForItemAsync(cid, ct);
                var set = rows
                    .Where(r => r.VariantAttributeId.HasValue && !string.IsNullOrWhiteSpace(r.OptionValue))
                    .Select(r => (r.VariantAttributeId!.Value, Normalize(r.OptionValue)))
                    .ToHashSet();
                if (set.Count > 0) existingCombos.Add(set);
            }

            // 4) Group payload by Combo (use NORMALIZED values!)
            var groups = normalized
                .GroupBy(v => v.Combo ?? 1)
                .OrderBy(g => g.Key)
                .ToList();

            int lastCreatedId = 0;
            bool createdAny   = false;

            foreach (var g in groups)
            {
                // Dedupe by attribute within the combo; keep the last for that attribute
                var sparse = g
                    .GroupBy(v => v.VariantAttributeId!.Value)
                    .Select(gg =>
                    {
                        var last = gg.Last();
                        return (VarAttrId: last.VariantAttributeId!.Value, Opt: Normalize(last.OptionValue));
                    })
                    .ToList();

                if (sparse.Count == 0) continue;

                var sparseSet = sparse.ToHashSet();

                // Overlap protection (equal/subset/superset)
                if (existingCombos.Any(ex => IsSubset(ex, sparseSet) || IsSubset(sparseSet, ex)))
                {
                    var label = string.Join(", ", sparse.OrderBy(x => x.VarAttrId).Select(x => $"{x.VarAttrId}:{x.Opt}"));
                    throw new InvalidOperationException($"A variant overlapping this selection already exists ({label}).");
                }

                // Build ordered selections only for provided attributes (sparse)
                var orderedSelections = orderedAttrIds
                    .Where(id => sparseSet.Any(p => p.VarAttrId == id))
                    .Select(id => new VariantValueDto
                    {
                        VariantAttributeId = id,
                        OptionValue = sparseSet.First(p => p.VarAttrId == id).Opt
                    })
                    .ToList();

                // Code & Name
                var tokens   = orderedSelections.Select(v => Tokenize(v.OptionValue)).ToList();
                var baseCode = $"{template.ItemCode}-{string.Join("-", tokens)}";
                var finalCode = await NextChildCodeAsync(template.ItemCode, ct);

                var labelPairs = attrs
                    .OrderBy(a => a.Order)
                    .Select(a =>
                    {
                        var sel = orderedSelections.FirstOrDefault(v => v.VariantAttributeId == a.Id);
                        if (sel == null || string.IsNullOrWhiteSpace(sel.OptionValue)) return null;

                        var label = (a.AttributeName ?? $"attr{a.AttributeId}").Trim();
                        var value = sel.OptionValue.Trim();
                        return $"{label.ToLowerInvariant()}:{value}";
                    })
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .ToList();

                var childName = labelPairs.Count > 0
                    ? $"{template.ItemName} - {string.Join("-", labelPairs)}"
                    : template.ItemName;

                // Create child + selections
                var childId = await _uow.ExecuteInTransactionAsync<int>(async _ =>
                {
                    var child = new ItemMaster
                    {
                        UnitId = template.UnitId,
                        ItemCode = finalCode,
                        ItemName = childName,
                        HSNId = template.HSNId,
                        ItemGroupId = template.ItemGroupId,
                        ItemCategoryId = template.ItemCategoryId,
                        StockUomId = template.StockUomId,
                        ItemClassificationId = template.ItemClassificationId,
                        Description = template.Description,
                        ValidFrom = template.ValidFrom,
                        XPlantMaterialStatusId = template.XPlantMaterialStatusId,
                        IsStockItem = template.IsStockItem,
                        IsCapitalItem = template.IsCapitalItem,
                        MaintainStock = template.MaintainStock,
                        HasVariants = false,
                        ParentItemId = template.Id,
                        IsActive = BaseEntity.Status.Active,
                        IsDeleted = template.IsDeleted,
                        IssueRuleId = template.IssueRuleId,
                        IsOnSpot = template.IsOnSpot
                    };
                    var newId = await _itemRepo.CreateAsync(child, ct);

                    await CloneTabsAndCollectionsAsync(newId, p, template, ct);
                    await _variantValCmd.UpsertListAsync(newId, orderedSelections, ct);

                    return newId;
                }, ct);

                existingCombos.Add(sparseSet);
                lastCreatedId = childId;
                createdAny    = true;
            }

            if (!createdAny)
                throw new InvalidOperationException("No child variants were created. Ensure each selection references valid template attributes or row ids.");

            return lastCreatedId;

            // locals
            static string Normalize(string s) => (s ?? string.Empty).Trim().ToLowerInvariant();
            static bool IsSubset(HashSet<(int VarAttrId, string Opt)> a, HashSet<(int VarAttrId, string Opt)> b)
                => a.All(p => b.Contains(p));
        }

        // ---------------- SINGLE ITEM ----------------
        private async Task<int> CreateSingleItemAsync(ItemDto p, CancellationToken ct)
        {
            if (!p.ItemGroupId.HasValue || !p.ItemCategoryId.HasValue)
                throw new InvalidOperationException("ItemGroupId and ItemCategoryId are required.");

            var itemCode = await _itemQry.GetLatestItemCode(p.ItemGroupId.Value, p.ItemCategoryId.Value, ct)
                         ?? throw new InvalidOperationException("Failed to generate ItemCode.");

            if (await _itemRepo.ExistsByCodeForCreateAsync(itemCode, ct))
                throw new InvalidOperationException($"Generated ItemCode '{itemCode}' already exists.");

            var itemId = await _uow.ExecuteInTransactionAsync<int>(async _ =>
            {
                var item = _mapper.Map<ItemMaster>(p);
                item.ItemCode = itemCode;
                item.HasVariants = false;
                item.IsActive = BaseEntity.Status.Active;

                var newId = await _itemRepo.CreateAsync(item, ct);

                if (p.Purchase is not null) { var e = _mapper.Map<ItemPurchase>(p.Purchase); e.ItemId = newId; await _purchaseRepo.CreateAsync(e, ct); }
                if (p.Inventory is not null) { var e = _mapper.Map<ItemInventory>(p.Inventory); e.ItemId = newId; await _inventoryRepo.CreateAsync(e, ct); }
                if (p.Quality is not null) { var e = _mapper.Map<ItemQuality>(p.Quality); e.ItemId = newId; await _qualityRepo.CreateAsync(e, ct); }
                if (p.Sale is not null) { var e = _mapper.Map<ItemSale>(p.Sale); e.ItemId = newId; await _saleRepo.CreateAsync(e, ct); }

                if (p.Suppliers.Count > 0) await _supplierRepo.UpdateAsync(newId, p.Suppliers, ct);
                if (p.Manufacture.Count > 0) await _manufactureRepo.UpdateAsync(newId, p.Manufacture, ct);
                if (p.Uoms.Count > 0) await _uomRepo.UpdateAsync(newId, p.Uoms, ct);

                return newId;
            }, ct);

            await _mediator.Publish(new AuditLogsDomainEvent(
                "Create",
                itemId.ToString(),
                p.ItemName,
                "Single item created (no variants).",
                "ItemMaster"), ct);

            if (!string.IsNullOrWhiteSpace(p.ItemImage))
                await TryMoveImageAsync(itemId, p.ItemImage!, itemCode, ct);

            return itemId;
        }

        // ============================== helpers ==============================


        private static string Tokenize(string s)
        {
            var raw = (s ?? string.Empty).Trim();
            var sb = new StringBuilder(raw.Length);
            foreach (var ch in raw)
            {
                if (char.IsLetterOrDigit(ch) || ch == '-') sb.Append(char.ToUpperInvariant(ch));
                else if (char.IsWhiteSpace(ch) || ch == '_' || ch == '/') sb.Append('-');
            }
            var token = sb.ToString().Trim('-');
            return string.IsNullOrEmpty(token) ? "X" : token;
        }
        private async Task<string> NextChildCodeAsync(string templateCode, CancellationToken ct)
        {
            // Start at 1 → RAW-COT-66-001
            for (int seq = 1; ; seq++)
            {
                var candidate = $"{templateCode}-{seq:000}";
                if (!await _itemRepo.ExistsByCodeForCreateAsync(candidate, ct))
                    return candidate;
            }
        }    

        private async Task CloneTabsAndCollectionsAsync(int childId, ItemDto payload, ItemMaster template, CancellationToken ct)
        {
            if (payload.Purchase is not null)
            if (!DtoEmptyChecker.IsEmpty(payload.Purchase))
            {
                var e = _mapper.Map<ItemPurchase>(payload.Purchase);
                e.ItemId = childId; 
                await _purchaseRepo.CreateAsync(e, ct);
            }
            if (!DtoEmptyChecker.IsEmpty(payload.Inventory))
            {
                var e = _mapper.Map<ItemInventory>(payload.Inventory);
                e.ItemId = childId; 
                await _inventoryRepo.CreateAsync(e, ct);
            }
            if (!DtoEmptyChecker.IsEmpty(payload.Quality))
            {
                var e = _mapper.Map<ItemQuality>(payload.Quality);
                e.ItemId = childId;
                await _qualityRepo.CreateAsync(e, ct);
            }
            if (!DtoEmptyChecker.IsEmpty(payload.Sale))
            {
                var e = _mapper.Map<ItemSale>(payload.Sale);
                e.ItemId = childId;
                await _saleRepo.CreateAsync(e, ct);
            }

            if (DtoEmptyChecker.HasAny(payload.Suppliers))   await _supplierRepo.UpdateAsync(childId, payload.Suppliers, ct);
            if (DtoEmptyChecker.HasAny(payload.Manufacture)) await _manufactureRepo.UpdateAsync(childId, payload.Manufacture, ct);
            if (DtoEmptyChecker.HasAny(payload.Uoms))        await _uomRepo.UpdateAsync(childId, payload.Uoms, ct);
        }

        private async Task TryMoveImageAsync(int itemId, string tempFileName, string baseCode, CancellationToken ct)
        {
            try
            {
                var baseDirectory = await _itemQry.GetBaseDirectoryAsync();
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", baseDirectory);

                var tempFullPath = Path.Combine(uploadPath, tempFileName);
                EnsureDirectoryExists(Path.GetDirectoryName(tempFullPath)!);

                if (File.Exists(tempFullPath))
                {
                    var newFile = $"{baseCode}{Path.GetExtension(tempFullPath)}";
                    var newPath = Path.Combine(Path.GetDirectoryName(tempFullPath)!, newFile);

                    File.Move(tempFullPath, newPath, overwrite: true);
                    await _itemRepo.UpdateItemImageAsync(itemId, newFile, ct);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Image move/rename failed for ItemId={ItemId}", itemId);
            }
        }

        private static void EnsureDirectoryExists(string path)
        {
            if (!string.IsNullOrEmpty(path) && !Directory.Exists(path))
                Directory.CreateDirectory(path);
        }
        public static class DtoEmptyChecker
        {           
            public static bool IsEmpty(object dto, params string[] ignoreProps)
            {
                if (dto is null) return true;

                var ignore = new HashSet<string>(ignoreProps ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase)
                {
                    "Id", "ItemId", "CreatedBy", "CreatedDate", "ModifiedBy", "ModifiedDate"
                };

                foreach (var p in dto.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
                {
                    if (!p.CanRead) continue;
                    if (ignore.Contains(p.Name)) continue;

                    var val = p.GetValue(dto);

                    if (val is null) continue;

                    // string
                    if (val is string s)
                    {
                        if (!string.IsNullOrWhiteSpace(s)) return false;
                        continue;
                    }

                    // IEnumerable (but not string)
                    if (val is System.Collections.IEnumerable seq && val is not string)
                    {
                        var hasAny = seq.GetEnumerator().MoveNext();
                        if (hasAny) return false;
                        continue;
                    }

                    var t = p.PropertyType;
                    // Nullable<T>
                    if (Nullable.GetUnderlyingType(t) is not null)
                    {
                        // has value (we already checked null above) ⇒ not empty
                        return false;
                    }

                    // ValueType vs default(T)
                    if (t.IsValueType)
                    {
                        var defaultVal = Activator.CreateInstance(t);
                        if (!val.Equals(defaultVal)) return false;
                        continue;
                    }

                    // Any other reference type (non-null) ⇒ not empty
                    return false;
                }

                return true;
            }

            public static bool HasAny<T>(IEnumerable<T> list) => list != null && list.Any();
        }

    }
}
