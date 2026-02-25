using Contracts.Dtos.Inventory;
using Contracts.Dtos.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Inventory;
using InventoryManagement.Application.Item.PutAway.Queries.GetPutAwayRuleItemId;
using InventoryManagement.Application.Common.Interfaces.Item.PutAway;

namespace InventoryManagement.Infrastructure.Repositories.Lookups
{
    internal sealed class PutawayRuleLookupRepository : IPutawayRuleLookup
    {
        private readonly IPutAwayRuleQueryRepository _putAwayRuleQueryRepository;

        public PutawayRuleLookupRepository(IPutAwayRuleQueryRepository putAwayRuleQueryRepository)
        {
            _putAwayRuleQueryRepository = putAwayRuleQueryRepository;
        }

        public async Task<IReadOnlyList<PutawayRuleLookupDto>> GetByIdsAsync(
            IEnumerable<int> itemIds,
            IEnumerable<int> warehouseIds,
            CancellationToken ct = default)
        {
            var filteredItems = itemIds?.Where(id => id > 0).Distinct().ToList() ?? new List<int>();
            var filteredWarehouses = warehouseIds?.Where(id => id > 0).Distinct().ToList() ?? new List<int>();

            if (filteredItems.Count == 0 || filteredWarehouses.Count == 0)
                return Array.Empty<PutawayRuleLookupDto>();

            var rules = await _putAwayRuleQueryRepository.GetPutAwayRuleDetailsAsync(filteredItems, filteredWarehouses);
            if (rules == null || rules.Count == 0)
                return Array.Empty<PutawayRuleLookupDto>();

            return rules
                .Where(dto => dto != null)
                .Select(dto => MapToLookupDto(dto!))
                .ToList();
        }

        public async Task<List<PutawayRuleDto>> GetPutAwayRuleDetailsAsync(
            List<int> itemIds,
            List<int> warehouseIds,
            CancellationToken ct = default)
        {
            var filteredItems = itemIds?.Where(id => id > 0).Distinct().ToList() ?? new List<int>();
            var filteredWarehouses = warehouseIds?.Where(id => id > 0).Distinct().ToList() ?? new List<int>();

            if (filteredItems.Count == 0 || filteredWarehouses.Count == 0)
                return new List<PutawayRuleDto>();

            var rules = await _putAwayRuleQueryRepository.GetPutAwayRuleDetailsAsync(filteredItems, filteredWarehouses);
            if (rules == null || rules.Count == 0)
                return new List<PutawayRuleDto>();

            return rules
                .Where(dto => dto != null)
                .Select(dto => MapToPutawayRuleDto(dto!))
                .ToList();
        }

        public async Task<List<PutawayRuleDto>> GetPutAwayRuleDetailsByWarehouseAsync(
            List<int> itemIds,
            List<int> warehouseIds,
            CancellationToken ct = default)
        {
            var filteredItems = itemIds?.Where(id => id > 0).Distinct().ToList() ?? new List<int>();
            var filteredWarehouses = warehouseIds?.Where(id => id > 0).Distinct().ToList() ?? new List<int>();

            if (filteredItems.Count == 0 || filteredWarehouses.Count == 0)
                return new List<PutawayRuleDto>();

            var rules = await _putAwayRuleQueryRepository.GetPutAwayRuleWarehouseDetailsAsync(filteredItems, filteredWarehouses);
            if (rules == null || rules.Count == 0)
                return new List<PutawayRuleDto>();

            return rules
                .Where(dto => dto != null)
                .Select(dto => MapToPutawayRuleDto(dto!))
                .ToList();
        }

        private static PutawayRuleLookupDto MapToLookupDto(GetPutAwayRuleItemIdDto dto) => new PutawayRuleLookupDto
        {
            PutAwayRuleId = dto.PutAwayRuleId ?? 0,
            StorageTypeId = dto.StorageTypeId,
            StorageTypeName = dto.StorageTypeName ?? string.Empty,
            TargetId = dto.TargetId,
            TargetCode = dto.TargetCode ?? string.Empty,
            TargetName = dto.TargetName ?? string.Empty,
            PriorityId = dto.PriorityId,
            PriorityName = dto.PriorityName ?? string.Empty,
            WarehouseId = dto.WarehouseId,
            WarehouseCode = dto.WarehouseCode ?? string.Empty,
            WarehouseName = dto.WarehouseName ?? string.Empty,
            UnitId = dto.UnitId,
            ItemId = dto.ItemId,
            ItemCode = dto.ItemCode ?? string.Empty,
            RuleItemId = dto.RuleItemId,
            ItemCategoryId = dto.ItemCategoryId,
            ItemCategoryName = dto.ItemCategoryName ?? string.Empty,
            ItemGroupId = dto.ItemGroupId,
            ItemGroupName = dto.ItemGroupName ?? string.Empty,
            StockUomId = dto.StockUomId,
            StockUom = dto.StockUom ?? string.Empty,
            PurchaseUomId = dto.PurchaseUomId,
            PurchaseUom = dto.PurchaseUom ?? string.Empty,
            ItemName = dto.ItemName ?? string.Empty,
            ConversionRate = dto.ConversionRate
        };

        private static PutawayRuleDto MapToPutawayRuleDto(GetPutAwayRuleItemIdDto dto) => new PutawayRuleDto
        {
            PutAwayRuleId = dto.PutAwayRuleId ?? 0,
            StorageTypeId = dto.StorageTypeId,
            StorageTypeName = dto.StorageTypeName ?? string.Empty,
            TargetId = dto.TargetId,
            TargetCode = dto.TargetCode ?? string.Empty,
            TargetName = dto.TargetName ?? string.Empty,
            PriorityId = dto.PriorityId,
            PriorityName = dto.PriorityName ?? string.Empty,
            WarehouseId = dto.WarehouseId,
            WarehouseCode = dto.WarehouseCode ?? string.Empty,
            WarehouseName = dto.WarehouseName ?? string.Empty,
            UnitId = dto.UnitId,
            ItemId = dto.ItemId,
            ItemCode = dto.ItemCode ?? string.Empty,
            RuleItemId = dto.RuleItemId ?? 0,
            ItemCategoryId = dto.ItemCategoryId ?? 0,
            ItemCategoryName = dto.ItemCategoryName ?? string.Empty,
            ItemGroupId = dto.ItemGroupId,
            ItemGroupName = dto.ItemGroupName ?? string.Empty,
            StockUomId = dto.StockUomId ?? 0,
            StockUom = dto.StockUom ?? string.Empty,
            PurchaseUomId = dto.PurchaseUomId ?? 0,
            PurchaseUom = dto.PurchaseUom ?? string.Empty,
            ItemName = dto.ItemName ?? string.Empty,
            ConversionRate = dto.ConversionRate
        };
    }
}
