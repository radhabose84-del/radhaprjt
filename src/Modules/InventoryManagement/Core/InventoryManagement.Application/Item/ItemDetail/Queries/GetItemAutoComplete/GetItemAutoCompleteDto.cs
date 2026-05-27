namespace InventoryManagement.Application.Item.ItemDetail.Queries.GetItemAutoComplete
{
    public class ItemVendorDto
    {
        public int SupplierId { get; set; }
        public int UnitId { get; set; }
        public string? SupplierPartNo { get; set; }
        public bool DefaultSupplier { get; set; }
        public int LeadTime { get; set; }
        public decimal MOQ { get; set; }
        public int MOQUomId { get; set; }
        public int PackageUomId { get; set; }
        public decimal PackageValue { get; set; }
    }

    public class GetItemAutoCompleteDto
    {
        public int Id { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public int HSNId { get; set; }
        public string? HSNCode { get; set; } = string.Empty;
        public decimal GSTPercentage { get; set; }
        public string? ParentItemId { get; set; }
        public int ItemCategoryId { get; set; }
        public int ItemGroupId { get; set; }
        public string? TariffNumber { get; set; }
        public int PurchaseUomId { get; set; }
        public string? PurchaseUom { get; set; }
        public int StockUomId { get; set; }
        public string? StockUom { get; set; }
        public bool IsOnSpot { get; set; }
        public int SourceOfItem { get; set; }
        public decimal? MinQuantity { get; set; }
        public int? SaleUomId { get; set; }
        public string? SaleUom { get; set; }
 		public List<ItemVendorDto>? Vendors { get; set; }
		public decimal? CurrentStockQty { get; set; }
        public int? EmergencyPOById { get; set; }
        public decimal? EmergencyValueLimit { get; set; }
        public int? EmergencyActionId { get; set; }
        // Populated only by GetItemsByVariantFilter — from Inventory.ItemInventory.DefaultPackTypeId.
        public int? DefaultPackTypeId { get; set; }
        // Populated only by GetItemsByVariantFilter — config rows for this item's category + current unit.
        public List<ItemCategoryUnitConfigDto>? ItemCategoryUnitConfigs { get; set; }
    }
}

