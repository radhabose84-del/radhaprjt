using InventoryManagement.Domain.Common;

namespace InventoryManagement.Domain.Entities.Item.ItemDetail
{
    public class ItemSupplier
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        public ItemMaster Item { get; set; } = null!;
        public int SupplierId { get; set; }
        public int UnitId { get; set; }
        public string? SupplierPartNo { get; set; }
        public int? LeadTime { get; set; }
        public int? MOQ { get; set; }
        public int? MOQUomId { get; set; }
        public decimal? PackageValue { get; set; }
        public int? PackageUomId { get; set; }
        public bool? DefaultSupplier { get; set; }
    }
}
