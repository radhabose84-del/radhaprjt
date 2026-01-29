using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Contracts.Dtos.Inventory
{
    public class PutawayRuleDto
    {
        public int PutAwayRuleId { get; set; }
        public int StorageTypeId { get; set; }
        public string StorageTypeName { get; set; }
        public int TargetId { get; set; }
        public string TargetCode { get; set; }
        public string TargetName { get; set; }
        public int PriorityId { get; set; }
        public string PriorityName { get; set; } = "";
        public int? WarehouseId { get; set; }
        public string WarehouseCode { get; set; }
        public string WarehouseName { get; set; }
        public int UnitId { get; set; }
        public int ItemId { get; set; }
        public string ItemCode { get; set; } = string.Empty; // ✅ Added for clarity
        public int RuleItemId { get; set; }
        public int ItemCategoryId { get; set; }
        public string ItemCategoryName { get; set; }
        public int? ItemGroupId { get; set; }
        public string ItemGroupName { get; set; }
        public int StockUomId { get; set; }
        public string StockUom { get; set; }
        public int PurchaseUomId { get; set; }
        public string PurchaseUom { get; set; }
        public string ItemName { get; set; }
        public double? ConversionRate { get; set; }

    }
}