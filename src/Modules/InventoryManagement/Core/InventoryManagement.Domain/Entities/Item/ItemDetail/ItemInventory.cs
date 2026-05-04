namespace InventoryManagement.Domain.Entities.Item.ItemDetail
{
    public class ItemInventory
    {
        public int Id { get; set; }
        public int ItemId { get; set; }
        public ItemMaster Item { get; set; } = null!; 
        public decimal? Weight { get; set; }
        public int? WeightUomId { get; set; }
        public UOM? WeightUOM { get; set; }
        public decimal? Length { get; set; }
        public decimal? Breadth { get; set; }
        public decimal? Height { get; set; }
        public decimal? Volume { get; set; }
        public int? DimensionUomId { get; set; }
        public UOM? DimensionUOM { get; set; }
        public int? DefaultMaterialRequestTypeId { get; set; }
        public MiscMaster? MiscDefaultMaterialRequestType { get; set; } 
        public int? ValuationMethodId { get; set; }
        public MiscMaster? MiscValuationMethod { get; set; } 
        public int? ShelfLife { get; set; }
        public decimal? UpperTolerance { get; set; }
        public decimal? LowerTolerance { get; set; }
        public string? BatchNumberSeries { get; set; }
        public string? SerialNumberSeries { get; set; }
        public int? ReorderLevel { get; set; }
        public int? ReorderQty { get; set; }
        public int? RequestTypeId { get; set; }
        public MiscMaster? MiscRequestType { get; set; }
        public int? SafetyStock { get; set; }
        public bool AllowNegativeStock { get; set; }
        public bool BatchManagement { get; set; }
        public bool ApplyBatchNumber { get; set; }
    }
}