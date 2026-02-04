using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.Application.Item.PutAway.Queries.GetAllPutAwayRule
{
    public sealed class PutAwayRuleListDto
    {
        public int Id { get; set; }
        public int UnitId { get; set; }

        public int ItemGroupId { get; set; }
        public string ItemGroupName { get; set; } = "";

        public int ItemCategoryId { get; set; }
        public string ItemCategoryName { get; set; } = "";

        public int? ItemId { get; set; }
        public string? ItemName { get; set; }

        public int WarehouseId { get; set; }
        public string WarehouseCode { get; set; } = "";
        public string WarehouseName { get; set; } = "";
        public Status IsActive { get; set; }
    }
}