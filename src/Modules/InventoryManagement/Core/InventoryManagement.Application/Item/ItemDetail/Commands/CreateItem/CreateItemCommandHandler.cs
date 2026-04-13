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
        private readonly IItemUsageTypeMappingCommandRepository _usageTypeMappingRepo;

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
            IItemVariantAttributeCommandRepository variantAttrCmd,
            IItemUsageTypeMappingCommandRepository usageTypeMappingRepo)
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
            _usageTypeMappingRepo = usageTypeMappingRepo;
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
                // Explicit assignment — AutoMapper ForAllMembers condition blocks nullable int mapping
                item.PriceGroupId = p.PriceGroupId.HasValue && p.PriceGroupId.Value > 0 ? p.PriceGroupId : null;

                var newId = await _itemRepo.CreateAsync(item, ct);

                // Cascade PriceGroupId to all existing variant children under this template
                // (no-op on fresh create — applies when children already exist).
                if (item.HasVariants)
                {
                    await _itemRepo.UpdatePriceGroupForChildrenAsync(newId, item.PriceGroupId, ct);
                }

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
                if (DtoEmptyChecker.HasAny(p.Suppliers))        await _supplierRepo.UpdateAsync(newId, p.Suppliers, ct);
                if (DtoEmptyChecker.HasAny(p.Manufacture))     await _manufactureRepo.UpdateAsync(newId, p.Manufacture, ct);
                if (DtoEmptyChecker.HasAny(p.Uoms))            await _uomRepo.UpdateAsync(newId, p.Uoms, ct);
                if (DtoEmptyChecker.HasAny(p.ItemUsageTypeMappings)) await _usageTypeMappingRepo.UpdateAsync(newId, p.ItemUsageTypeMappings, ct);
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

            // 1) Load template (tracked — we may need to flip HasVariants)
            var template = await _itemRepo.GetTrackingAsync(p.ParentItemId!.Value, ct)
                            ?? throw new InvalidOperationException("Template item not found.");

            // 2) Resolve SpecificationValueId → (SpecMasterId, SpecValueName)
            //    The payload's variantAttributeId is ignored — we derive the attribute
            //    from the spec value's owning spec master.
            var incomingSpecValueIds = p.VariantValues
                .Where(v => v != null && v.SpecificationValueId > 0)
                .Select(v => v.SpecificationValueId)
                .Distinct()
                .ToList();

            var specValueMap = await _variantAttrCmd.GetSpecificationValueMapAsync(incomingSpecValueIds, ct);
            if (specValueMap.Count == 0)
                throw new InvalidOperationException("None of the provided SpecificationValueIds were found.");

            var missingSpecValueIds = incomingSpecValueIds.Except(specValueMap.Keys).ToList();
            if (missingSpecValueIds.Count > 0)
                throw new InvalidOperationException(
                    $"SpecificationValueId(s) not found or deleted: {string.Join(", ", missingSpecValueIds)}.");

            // 3) Auto-provision variant attributes on the template for every distinct
            //    SpecMasterId referenced by the payload.
            var requiredSpecMasterIds = specValueMap.Values
                .Select(v => v.SpecMasterId)
                .Distinct()
                .ToList();

            var existingAttrs = await _variantAttrCmd.GetForItemAsync(template.Id, ct);
            var existingSpecMasterIds = existingAttrs.Select(a => a.SpecificationMasterId).ToHashSet();
            var missingSpecMasterIds = requiredSpecMasterIds.Except(existingSpecMasterIds).ToList();

            // Persist template promotion + any missing attributes in a single transaction
            // so the newly-created ItemVariantAttribute rows get assigned Ids before we
            // re-query them for the child creation loop.
            if (missingSpecMasterIds.Count > 0 || !template.HasVariants)
            {
                await _uow.ExecuteInTransactionAsync(async innerCt =>
                {
                    if (missingSpecMasterIds.Count > 0)
                    {
                        var nextOrder = existingAttrs.Count == 0 ? 1 : existingAttrs.Max(a => a.Order) + 1;
                        var toCreate = missingSpecMasterIds
                            .Select((specMasterId, idx) => new VariantAttributeDto
                            {
                                Id = 0,
                                SpecificationMasterId = specMasterId,
                                Order = nextOrder + idx
                            })
                            .ToList();

                        await _variantAttrCmd.UpsertAttributesAsync(template.Id, toCreate, innerCt);
                    }

                    // Promote template to HasVariants=true so subsequent calls behave consistently.
                    if (!template.HasVariants)
                        template.HasVariants = true;
                }, ct);
            }

            // 4) Re-load attributes so we have Ids for the newly-created rows.
            var attrs = await _variantAttrCmd.GetForItemAsync(template.Id, ct);
            if (attrs.Count == 0)
                throw new InvalidOperationException("Failed to provision variant attributes on template.");

            var attrIdBySpecMaster = attrs.ToDictionary(a => a.SpecificationMasterId, a => a.Id);
            var orderedAttrIds = attrs.OrderBy(a => a.Order).Select(a => a.Id).ToList();

            // 5) Existing combos from real children (overlap protection)
            var existingCombos = new List<HashSet<(int VarAttrId, int SpecValueId)>>();
            var childIds = await _itemRepo.GetChildIdsAsync(template.Id, ct);
            foreach (var cid in childIds)
            {
                var rows = await _variantValQry.GetForItemAsync(cid, ct);
                var set = rows
                    .Where(r => r.VariantAttributeId.HasValue && r.SpecificationValueId > 0)
                    .Select(r => (r.VariantAttributeId!.Value, r.SpecificationValueId))
                    .ToHashSet();
                if (set.Count > 0) existingCombos.Add(set);
            }

            // 6) Group incoming payload by Combo; each group = one child item
            var groups = p.VariantValues
                .Where(v => v != null && v.SpecificationValueId > 0 && specValueMap.ContainsKey(v.SpecificationValueId))
                .GroupBy(v => v.Combo ?? 1)
                .OrderBy(g => g.Key)
                .ToList();

            bool createdAny = false;

            // 7) Create ALL child items inside a SINGLE atomic transaction.
            //    If any child fails (FK violation, overlap, duplicate code, etc.) every
            //    child rolls back together — preventing zombie rows that would trigger
            //    false overlap errors on the next retry.
            var lastCreatedId = await _uow.ExecuteInTransactionAsync<int>(async innerCt =>
            {
                int lastId = 0;

                foreach (var g in groups)
                {
                    // For each spec value in the combo, derive the correct VariantAttributeId
                    // from the owning SpecMaster. Dedupe by attribute (last wins).
                    var sparse = g
                        .Select(v =>
                        {
                            var specInfo = specValueMap[v.SpecificationValueId];
                            var varAttrId = attrIdBySpecMaster[specInfo.SpecMasterId];
                            return (VarAttrId: varAttrId, SpecValueId: v.SpecificationValueId, SpecValueName: specInfo.SpecValueName);
                        })
                        .GroupBy(x => x.VarAttrId)
                        .Select(gg => gg.Last())
                        .ToList();

                    if (sparse.Count == 0) continue;

                    var sparseSet = sparse.Select(s => (s.VarAttrId, s.SpecValueId)).ToHashSet();

                    // Overlap protection (equal/subset/superset) — checked against both
                    // pre-existing children AND siblings created earlier in this same loop.
                    if (existingCombos.Any(ex => IsSubset(ex, sparseSet) || IsSubset(sparseSet, ex)))
                    {
                        var label = string.Join(", ", sparse.OrderBy(x => x.VarAttrId).Select(x => $"{x.VarAttrId}:{x.SpecValueId}"));
                        throw new InvalidOperationException($"A variant overlapping this selection already exists ({label}).");
                    }

                    // Ordered selections for naming and persistence
                    var orderedSelections = orderedAttrIds
                        .Where(id => sparse.Any(s => s.VarAttrId == id))
                        .Select(id =>
                        {
                            var match = sparse.First(s => s.VarAttrId == id);
                            return new VariantValueDto
                            {
                                VariantAttributeId = id,
                                SpecificationValueId = match.SpecValueId,
                                SpecificationValue = match.SpecValueName
                            };
                        })
                        .ToList();

                    // Code & Name
                    var finalCode = await NextChildCodeAsync(template.ItemCode, innerCt);

                    var orderedValueNames = orderedSelections
                        .Select(v => v.SpecificationValue?.Trim())
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .ToList();

                    var childName = orderedValueNames.Count > 0
                        ? $"{template.ItemName} - {string.Join(" ", orderedValueNames)}"
                        : template.ItemName;

                    var child = new ItemMaster
                    {
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
                        IsOnSpot = template.IsOnSpot,
                        PriceGroupId = template.PriceGroupId
                    };
                    var newId = await _itemRepo.CreateAsync(child, innerCt);

                    await CloneTabsAndCollectionsAsync(newId, p, template, innerCt);
                    await _variantValCmd.UpsertListAsync(newId, orderedSelections, innerCt);

                    existingCombos.Add(sparseSet);
                    lastId = newId;
                    createdAny = true;
                }

                return lastId;
            }, ct);

            if (!createdAny)
                throw new InvalidOperationException("No child variants were created. Ensure each selection references valid specification values.");

            return lastCreatedId;

            // locals
            static bool IsSubset(HashSet<(int VarAttrId, int SpecValueId)> a, HashSet<(int VarAttrId, int SpecValueId)> b)
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
                // Explicit assignment — AutoMapper ForAllMembers condition blocks nullable int mapping
                item.PriceGroupId = p.PriceGroupId.HasValue && p.PriceGroupId.Value > 0 ? p.PriceGroupId : null;

                var newId = await _itemRepo.CreateAsync(item, ct);

                if (p.Purchase is not null) { var e = _mapper.Map<ItemPurchase>(p.Purchase); e.ItemId = newId; await _purchaseRepo.CreateAsync(e, ct); }
                if (p.Inventory is not null) { var e = _mapper.Map<ItemInventory>(p.Inventory); e.ItemId = newId; await _inventoryRepo.CreateAsync(e, ct); }
                if (p.Quality is not null) { var e = _mapper.Map<ItemQuality>(p.Quality); e.ItemId = newId; await _qualityRepo.CreateAsync(e, ct); }
                if (p.Sale is not null) { var e = _mapper.Map<ItemSale>(p.Sale); e.ItemId = newId; await _saleRepo.CreateAsync(e, ct); }

                if (p.Suppliers.Count > 0) await _supplierRepo.UpdateAsync(newId, p.Suppliers, ct);
                if (p.Manufacture.Count > 0) await _manufactureRepo.UpdateAsync(newId, p.Manufacture, ct);
                if (p.Uoms.Count > 0) await _uomRepo.UpdateAsync(newId, p.Uoms, ct);
                if (p.ItemUsageTypeMappings.Count > 0) await _usageTypeMappingRepo.UpdateAsync(newId, p.ItemUsageTypeMappings, ct);

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

            if (DtoEmptyChecker.HasAny(payload.Suppliers))        await _supplierRepo.UpdateAsync(childId, payload.Suppliers, ct);
            if (DtoEmptyChecker.HasAny(payload.Manufacture))     await _manufactureRepo.UpdateAsync(childId, payload.Manufacture, ct);
            if (DtoEmptyChecker.HasAny(payload.Uoms))            await _uomRepo.UpdateAsync(childId, payload.Uoms, ct);
            if (DtoEmptyChecker.HasAny(payload.ItemUsageTypeMappings)) await _usageTypeMappingRepo.UpdateAsync(childId, payload.ItemUsageTypeMappings, ct);
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
