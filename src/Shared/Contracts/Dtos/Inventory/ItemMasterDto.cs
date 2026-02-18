using System.Collections.Generic;

namespace Contracts.Dtos.Inventory
{
    public sealed class ItemMasterDto
    {
        public int Id { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string ItemCode { get; set; } = string.Empty;
        public int HSNId { get; set; }
        public string HSNCode { get; set; } = string.Empty;
        public decimal GSTPercentage { get; set; }
        public int ItemCategoryId { get; set; }
        public int ItemGroupId { get; set; }
        public int SourceOfItem { get; set; }
        public string TariffNumber { get; set; } = default!;
        public bool IsOnSpot { get; set; }
        public List<ItemVendorDto>? Vendors { get; set; }
    }
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

}
