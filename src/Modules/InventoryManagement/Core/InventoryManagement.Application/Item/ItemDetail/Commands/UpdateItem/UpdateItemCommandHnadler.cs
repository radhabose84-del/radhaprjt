#nullable disable
using AutoMapper;
using InventoryManagement.Application.Common.Interfaces.Item.ItemDetail.Commands;
using InventoryManagement.Application.Common.Interfaces.Item.ItemDetail.Queries;
using InventoryManagement.Application.Item.ItemDetail.Queries.GetAllItems;
using InventoryManagement.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace InventoryManagement.Application.Item.ItemDetail.Commands.UpdateItem
{
    public sealed class UpdateItemCommandHandler : IRequestHandler<UpdateItemCommand, Unit>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly ILogger<UpdateItemCommandHandler> _logger;

        private readonly IItemCommandRepository _itemRepo;
        private readonly IItemPurchaseCommandRepository _purchaseRepo;
        private readonly IItemInventoryCommandRepository _inventoryRepo;
        private readonly IItemQualityCommandRepository _qualityRepo;
        private readonly IItemSaleCommandRepository _saleRepo;
        private readonly IItemSupplierCommandRepository _supplierRepo;
        private readonly IItemManufactureCommandRepository _manuRepo;
        private readonly IItemUomCommandRepository _uomRepo;
        private readonly IItemUsageTypeMappingCommandRepository _usageTypeMappingRepo;
        // Variants
        private readonly IItemVariantAttributeCommandRepository _attrRepo;   // upsert template attributes
        private readonly IItemVariantValueCommandRepository _variantCmd;     // (optional) insert template options
        private readonly IItemVariantValueQueryRepository _variantQry;       // (optional) read existing template options

        public UpdateItemCommandHandler(
            IUnitOfWork uow,
            IMapper mapper,
            IMediator mediator,
            ILogger<UpdateItemCommandHandler> logger,
            IItemCommandRepository itemRepo,
            IItemPurchaseCommandRepository purchaseRepo,
            IItemInventoryCommandRepository inventoryRepo,
            IItemQualityCommandRepository qualityRepo,
            IItemSaleCommandRepository saleRepo,
            IItemSupplierCommandRepository supplierRepo,
            IItemManufactureCommandRepository manuRepo,
            IItemUomCommandRepository uomRepo,
            IItemUsageTypeMappingCommandRepository usageTypeMappingRepo,
            IItemVariantAttributeCommandRepository attrRepo,
            IItemVariantValueCommandRepository variantCmd,
            IItemVariantValueQueryRepository variantQry)
        {
            _uow = uow; _mapper = mapper; _mediator = mediator; _logger = logger;
            _itemRepo = itemRepo; _purchaseRepo = purchaseRepo; _inventoryRepo = inventoryRepo; _qualityRepo = qualityRepo;
            _saleRepo = saleRepo; _supplierRepo = supplierRepo; _manuRepo = manuRepo; _uomRepo = uomRepo;
            _usageTypeMappingRepo = usageTypeMappingRepo; _attrRepo = attrRepo; _variantCmd = variantCmd; _variantQry = variantQry;
        }

        public async Task<Unit> Handle(UpdateItemCommand request, CancellationToken ct)
        {
            var p = request.Payload;

            await _uow.ExecuteInTransactionAsync<Unit>(async _ =>
            {
                // 1) Load + guards
                var item = await _itemRepo.GetTrackingAsync(p.Id, ct)
                           ?? throw new KeyNotFoundException("Item not found.");

                if (!string.IsNullOrWhiteSpace(p.ItemCode) &&
                    await _itemRepo.ExistsByCodeForUpdateAsync(p.ItemCode, item.Id, ct))
                    throw new InvalidOperationException("Another item with the same ItemCode exists.");

                // 2) Base update (mapper profile must ignore entity keys/navs)
                _mapper.Map(p, item);
                await _itemRepo.UpdateAsync(item, ct);

                // 3) Tabs (upsert)
                if (p.Purchase is not null)
                {
                    var row = await _purchaseRepo.GetByItemIdAsync(item.Id, ct);
                    if (row is null)
                    {
                        var add = _mapper.Map<InventoryManagement.Domain.Entities.Item.ItemDetail.ItemPurchase>(p.Purchase);
                        add.ItemId = item.Id;
                        await _purchaseRepo.CreateAsync(add, ct);
                    }
                    else
                    {
                        _mapper.Map(p.Purchase, row);
                        await _purchaseRepo.UpdateAsync(row, ct);
                    }
                }

                if (p.Inventory is not null)
                {
                    var row = await _inventoryRepo.GetByItemIdAsync(item.Id, ct);
                    if (row is null)
                    {
                        var add = _mapper.Map<InventoryManagement.Domain.Entities.Item.ItemDetail.ItemInventory>(p.Inventory);
                        add.ItemId = item.Id;
                        await _inventoryRepo.CreateAsync(add, ct);
                    }
                    else
                    {
                        _mapper.Map(p.Inventory, row);
                        await _inventoryRepo.UpdateAsync(row, ct);
                    }
                }

                if (p.Quality is not null)
                {
                    var row = await _qualityRepo.GetByItemIdAsync(item.Id, ct);
                    if (row is null)
                    {
                        var add = _mapper.Map<InventoryManagement.Domain.Entities.Item.ItemDetail.ItemQuality>(p.Quality);
                        add.ItemId = item.Id;
                        await _qualityRepo.CreateAsync(add, ct);
                    }
                    else
                    {
                        _mapper.Map(p.Quality, row);
                        await _qualityRepo.UpdateAsync(row, ct);
                    }
                }

                if (p.Sale is not null)
                {
                    var row = await _saleRepo.GetByItemIdAsync(item.Id, ct);
                    if (row is null)
                    {
                        var add = _mapper.Map<InventoryManagement.Domain.Entities.Item.ItemDetail.ItemSale>(p.Sale);
                        add.ItemId = item.Id;
                        await _saleRepo.CreateAsync(add, ct);
                    }
                    else
                    {
                        _mapper.Map(p.Sale, row);
                        await _saleRepo.UpdateAsync(row, ct);
                    }
                }

                // 4) Collections
                if (p.Suppliers is not null)        await _supplierRepo.UpdateAsync(item.Id, p.Suppliers, ct);
                if (p.Manufacture is not null)      await _manuRepo.UpdateAsync(item.Id, p.Manufacture, ct);
                if (p.Uoms is not null)             await _uomRepo.UpdateAsync(item.Id, p.Uoms, ct);
                if (p.ItemUsageTypeMappings is not null)  await _usageTypeMappingRepo.UpdateAsync(item.Id, p.ItemUsageTypeMappings, ct);

                // 5) VARIANT ATTRIBUTES (this is the missing piece)
                // Turn the item into a template / maintain template attributes
                if (p.HasVariants && p.VariantAttributes is { Count: > 0 })
                {                   
                    await _attrRepo.UpsertAttributesAsync(item.Id, p.VariantAttributes, ct);
                }

                // 6) (Optional) TEMPLATE-LEVEL ALLOWED VALUES – add-only
                // If your schema stores allowed options for the template in ItemVariantValue (ItemId = template),
                // keep this block. If you store values only on children, you can remove it safely.
                if (p.HasVariants && p.VariantValues is { Count: > 0 })
                {
                    var currentByVarAttr = await _variantQry.GetForItemGroupedAsync(item.Id, ct);

                    var toAdd = new List<VariantValueDto>();
                    foreach (var v in p.VariantValues)
                    {
                        if (v?.VariantAttributeId is not int varAttrId || varAttrId <= 0) continue;
                        if (string.IsNullOrWhiteSpace(v.OptionValue)) continue;

                        var opt = v.OptionValue.Trim();
                        var exists = currentByVarAttr.TryGetValue(varAttrId, out var list)
                                  && list.Any(x => string.Equals(x, opt, StringComparison.OrdinalIgnoreCase));

                        if (!exists)
                            toAdd.Add(new VariantValueDto { VariantAttributeId = varAttrId, OptionValue = opt });
                    }

                    if (toAdd.Count > 0)
                        await _variantCmd.AddMissingTemplateOptionsAsync(item.Id, toAdd, ct); // insert-only
                }

                return Unit.Value; // commit
            }, ct);

            await _mediator.Publish(new AuditLogsDomainEvent(
                "Update",
                request.Payload.Id.ToString(),
                request.Payload.ItemName,
                "Item updated; template attributes upserted; template options appended (if provided).",
                "ItemMaster"), ct);

            return Unit.Value;
        }
    }
}
