namespace ProductionManagement.Application.ProductionPack.Dto
{
    public class ProductionPackHeaderDto
    {
        public int Id { get; set; }
        public string? PackNo { get; set; }
        public DateOnly PackDate { get; set; }
        public int ProductionYear { get; set; }
        public int UnitId { get; set; }
        public string? UnitName { get; set; }
        public int WarehouseId { get; set; }
        public string? WarehouseName { get; set; }

        // Item identification
        public int ItemId { get; set; }
        public string? ItemName { get; set; }
        public int? VariantId { get; set; }
        public string? VariantName { get; set; }

        // Location & quality
        public int? BinId { get; set; }
        public string? BinName { get; set; }
        public int? QualityStatusId { get; set; }
        public string? QualityStatusName { get; set; }

        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public string? CreatedByName { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public string? ModifiedByName { get; set; }

        // Detail rows (lot-level)
        public List<ProductionPackDetailDto>? Details { get; set; }
    }
}
