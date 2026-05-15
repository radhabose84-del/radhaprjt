namespace InventoryManagement.Application.Item.ItemDetail.Queries.GetItemAutoComplete
{
    // One row of Inventory.ItemCategoryUnitConfig for the item's category, scoped to the
    // caller's current Unit (resolved from IIPAddressService). Active + non-deleted only.
    public class ItemCategoryUnitConfigDto
    {
        public int ItemCategoryId { get; set; }
        public int UomId { get; set; }
        public string? UomName { get; set; }
        public int UnitId { get; set; }
        public decimal MaxSampleQuantity { get; set; }
    }
}
