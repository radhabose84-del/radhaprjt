namespace InventoryManagement.Application.Item.ItemDetail.Queries.GetAllItems
{
    public sealed class ItemListDto
    {
        public int Id { get; set; }
        public string ItemCode { get; set; } = null!;
        public string ItemName { get; set; } = null!;
        public bool HasVariants { get; set; }
        public bool IsStockItem { get; set; }
        public bool IsCapitalItem { get; set; }
        public string? ParentItemName { get; set; }
        public string? ItemGroupName { get; set; }
        public string? ItemCategoryName { get; set; }
        public string? UOMName { get; set; }
        public int? UOMId { get; set; }
        public int HsnId { get; set; }
        public int IsActive { get; set; }
        public bool IsOnSpot { get; set; }
        public int? IssueRuleId { get; set; }
        public string? IssueRule { get; set; }
        public int? PriceGroupId { get; set; }
        public string? PriceGroupCode { get; set; }
        public string? PriceGroupName { get; set; }
    }
}