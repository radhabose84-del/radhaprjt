namespace Contracts.Dtos.Inventory
{
    public class PutawayRuleDto
    {
        public int PutAwayRuleId { get; set; }
        public int StorageTypeId { get; set; }
        public string StorageTypeName { get; set; } = default!;
        public int TargetId { get; set; }
        public string TargetCode { get; set; } = default!;
        public string TargetName { get; set; } = default!;
        public int PriorityId { get; set; }
        public string PriorityName { get; set; } = "";
        public int? WarehouseId { get; set; }
        public string WarehouseCode { get; set; } = default!;
        public string WarehouseName { get; set; } = default!;
        public int UnitId { get; set; }
        public int ItemId { get; set; }
        public string ItemCode { get; set; } = string.Empty; // ✅ Added for clarity
        public int RuleItemId { get; set; }
        public int ItemCategoryId { get; set; }
        public string ItemCategoryName { get; set; } = default!;
        public int? ItemGroupId { get; set; }
        public string ItemGroupName { get; set; } = default!;
        public int StockUomId { get; set; }
        public string StockUom { get; set; } = default!;
        public int PurchaseUomId { get; set; }
        public string PurchaseUom { get; set; } = default!;
        public string ItemName { get; set; } = default!;
        public double? ConversionRate { get; set; }

    }
}