using Contracts.Dtos.Inventory;

namespace Contracts.Dtos.Lookups.Inventory
{
    public class ItemLookupDto
    {
        public int Id { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string ItemName { get; set; } = string.Empty;
        public int? ParentItemId { get; set; }
        public string? ParentItemName { get; set; }
        public string? TariffNumber { get; set; }
        public string? HSNCode { get; set; }
        public decimal GSTPercentage { get; set; }
        public bool IsOnSpot { get; set; }
        public int SourceOfItem { get; set; }
        public int ItemCategoryId { get; set; }
        public bool InspectionRequired { get; set; }
        public List<ItemVendorDto>? Vendors { get; set; }
    }
}
