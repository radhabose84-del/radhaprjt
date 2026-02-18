
namespace Contracts.Dtos.Lookups.Inventory
{
    public class PutawayRuleLookupDto
    {
        public int PutAwayRuleId { get; set; }
        public int StorageTypeId { get; set; }
        public string StorageTypeName { get; set; } = string.Empty;
        public int TargetId { get; set; }
        public string TargetCode { get; set; } = string.Empty;
        public string TargetName { get; set; } = string.Empty;
        public int PriorityId { get; set; }
        public string PriorityName { get; set; } = string.Empty;
        public int? WarehouseId { get; set; }
        public string WarehouseCode { get; set; } = string.Empty;
        public string WarehouseName { get; set; } = string.Empty;
        public int UnitId { get; set; }
        public int ItemId { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public int? RuleItemId { get; set; }
        public int? ItemCategoryId { get; set; }
        public string ItemCategoryName { get; set; } = string.Empty;
        public int? ItemGroupId { get; set; }
        public string ItemGroupName { get; set; } = string.Empty;
        public int? StockUomId { get; set; }
        public string StockUom { get; set; } = string.Empty;
        public int? PurchaseUomId { get; set; }
        public string PurchaseUom { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public double? ConversionRate { get; set; }
    }
}
