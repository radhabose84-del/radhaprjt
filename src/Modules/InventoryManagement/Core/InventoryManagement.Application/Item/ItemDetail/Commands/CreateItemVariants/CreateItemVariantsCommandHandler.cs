using System.Text;
using AutoMapper;
using InventoryManagement.Application.Common.Interfaces.Item.ItemDetail.Commands;
using InventoryManagement.Application.Common.Interfaces.Item.ItemDetail.Queries;
using InventoryManagement.Application.Item.ItemDetail.Queries.GetAllItems;
using InventoryManagement.Domain.Common;
using InventoryManagement.Domain.Entities.Item.ItemDetail;
using MediatR;

namespace InventoryManagement.Application.Item.ItemDetail.Commands.CreateItemVariants
{
    public sealed class CreateItemVariantsCommandHandler
        : IRequestHandler<CreateItemVariantsCommand, List<int>>
    {
        private readonly IItemCommandRepository _itemRepo;
        private readonly IItemVariantAttributeCommandRepository _attrRepo;
        private readonly IItemVariantValueCommandRepository _valueCmd;
        private readonly IItemVariantValueQueryRepository _valueQry;
        private readonly IItemQueryRepository _itemQry;

        private readonly IItemPurchaseCommandRepository _purchaseRepo;
        private readonly IItemInventoryCommandRepository _inventoryRepo;
        private readonly IItemQualityCommandRepository _qualityRepo;
        private readonly IItemSupplierCommandRepository _supplierRepo;
        private readonly IItemManufactureCommandRepository _manufactureRepo;
        private readonly IItemUomCommandRepository _uomRepo;

        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public CreateItemVariantsCommandHandler(
            IItemCommandRepository itemRepo,
            IItemVariantAttributeCommandRepository attrRepo,
            IItemVariantValueCommandRepository valueCmd,
            IItemVariantValueQueryRepository valueQry,
            IItemQueryRepository itemQry,
            IItemPurchaseCommandRepository purchaseRepo,
            IItemInventoryCommandRepository inventoryRepo,
            IItemQualityCommandRepository qualityRepo,
            IItemSupplierCommandRepository supplierRepo,
            IItemManufactureCommandRepository manufactureRepo,
            IItemUomCommandRepository uomRepo,
            IUnitOfWork uow,
            IMapper mapper)
        {
            _itemRepo = itemRepo;
            _attrRepo = attrRepo;
            _valueCmd = valueCmd;
            _valueQry = valueQry;
            _itemQry = itemQry;

            _purchaseRepo = purchaseRepo;
            _inventoryRepo = inventoryRepo;
            _qualityRepo = qualityRepo;
            _supplierRepo = supplierRepo;
            _manufactureRepo = manufactureRepo;
            _uomRepo = uomRepo;

            _uow = uow;
            _mapper = mapper;
        }

        public async Task<List<int>> Handle(CreateItemVariantsCommand request, CancellationToken ct)
        {
            var p = request.Payload;

            if (p.ParentItemId is null)
                throw new InvalidOperationException("ParentItemId (template) is required.");

            if (p.VariantValues is null || p.VariantValues.Count == 0)
                throw new InvalidOperationException("VariantValues are required.");

            var template = await _itemRepo.GetTrackingAsync(p.ParentItemId.Value, ct)
                           ?? throw new InvalidOperationException("Template not found.");
            if (!template.HasVariants)
                throw new InvalidOperationException("Parent is not a template.");

            var attrs = await _attrRepo.GetForItemAsync(template.Id, ct);
            if (attrs.Count == 0)
                throw new InvalidOperationException("Template has no attributes.");

            var existingKeys = await _valueQry.GetExistingChildComboKeysAsync(template.Id, ct);

            // Group flat values by Combo (each group => one variant child)
            var groups = p.VariantValues
                .Where(v => v != null && !string.IsNullOrWhiteSpace(v.OptionValue))
                .GroupBy(v => v.Combo ?? 1)
                .OrderBy(g => g.Key)
                .Select(g => g.ToList())
                .ToList();

            if (groups.Count == 0)
                throw new InvalidOperationException("VariantValues are empty.");

            var created = new List<int>();

            foreach (var values in groups)
            {
                // Order per template + bind VariantAttributeId
                var ordered = OrderAgainstTemplate(attrs, values);

                // Prevent duplicates under template               
                var comboKey = string.Join("|",
                    ordered.OrderBy(v => v.VariantAttributeId)
                        .Select(v => $"{v.VariantAttributeId}:{(v.OptionValue ?? "").Trim().ToLower()}"));
                if (existingKeys.Contains(comboKey))
                    throw new InvalidOperationException(
                        $"Variant already exists for ({string.Join(", ", ordered.Select(v => v.OptionValue))}).");


                // Build child code & name
                var tokens = ordered.Select(v => Tokenize(v.OptionValue)).ToList();
                var baseCode = $"{template.ItemCode}-{string.Join("-", tokens)}";
                var finalCode = await EnsureUniqueCodeAsync(baseCode, ct);
                var name = $"{template.ItemName} {string.Join(" / ", ordered.Select(v => v.OptionValue))}";

                var newId = await _uow.ExecuteInTransactionAsync<int>(async _ =>
                {
                    var child = new ItemMaster
                    {
                        UnitId = template.UnitId,
                        ItemCode = finalCode,
                        ItemName = name,
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
                    var childId = await _itemRepo.CreateAsync(child, ct);

                    // Optional overrides from payload
                    await CloneTabsAndCollectionsAsync(childId, p, template, ct);

                    // Persist selected values (each must have VariantAttributeId set)
                    await _valueCmd.UpsertChildSelectionsAsync(childId, ordered, ct);
                    return childId;
                }, ct);

                existingKeys.Add(comboKey);
                created.Add(newId);
            }

            return created;
        }

      private static List<VariantValueDto> OrderAgainstTemplate(
        List<VariantAttributeDto> attrs, List<VariantValueDto> values)
        {
            // incoming must carry VariantAttributeId for each selection
            var map = values
                .Where(v => v?.VariantAttributeId is int id && id > 0 && !string.IsNullOrWhiteSpace(v.OptionValue))
                .ToDictionary(v => v!.VariantAttributeId!.Value, v => v!);

            var ordered = new List<VariantValueDto>(attrs.Count);

            foreach (var a in attrs.OrderBy(x => x.Order))
            {
                if (!map.TryGetValue(a.Id, out var v))
                    throw new InvalidOperationException($"Missing value for attribute {a.AttributeId}.");
                // (v already has VariantAttributeId == a.Id)
                ordered.Add(v);
            }
            return ordered;
        }


        private static string Tokenize(string s)
        {
            var raw = (s ?? string.Empty).Trim();
            var sb = new StringBuilder();
            foreach (var ch in raw)
            {
                if (char.IsLetterOrDigit(ch) || ch == '-') sb.Append(char.ToUpperInvariant(ch));
                else if (char.IsWhiteSpace(ch) || ch == '_' || ch == '/') sb.Append('-');
            }
            var token = sb.ToString().Trim('-');
            return string.IsNullOrEmpty(token) ? "X" : token;
        }

        private async Task<string> EnsureUniqueCodeAsync(string baseCode, CancellationToken ct)
        {
            if (!await _itemRepo.ExistsByCodeForCreateAsync(baseCode, ct)) return baseCode;
            for (int i = 1; ; i++)
            {
                var cand = $"{baseCode}-{i:00}";
                if (!await _itemRepo.ExistsByCodeForCreateAsync(cand, ct)) return cand;
            }
        }

        private async Task CloneTabsAndCollectionsAsync(int childId, ItemDto payload, ItemMaster template, CancellationToken ct)
        {
            if (payload.Purchase is not null)
            {
                var e = _mapper.Map<ItemPurchase>(payload.Purchase);
                e.ItemId = childId; await _purchaseRepo.CreateAsync(e, ct);
            }
            if (payload.Inventory is not null)
            {
                var e = _mapper.Map<ItemInventory>(payload.Inventory);
                e.ItemId = childId; await _inventoryRepo.CreateAsync(e, ct);
            }
            if (payload.Quality is not null)
            {
                var e = _mapper.Map<ItemQuality>(payload.Quality);
                e.ItemId = childId; await _qualityRepo.CreateAsync(e, ct);
            }

            if (payload.Suppliers?.Count   > 0) await _supplierRepo.UpdateAsync(childId, payload.Suppliers, ct);
            if (payload.Manufacture?.Count > 0) await _manufactureRepo.UpdateAsync(childId, payload.Manufacture, ct);
            if (payload.Uoms?.Count        > 0) await _uomRepo.UpdateAsync(childId, payload.Uoms, ct);
        }
    }
}
