namespace SalesManagement.Application.ProductionPack.Dto
{
    public class ProductionPackHeaderDto
    {
        public int Id { get; set; }
        public string? PackNo { get; set; }
        public DateOnly PackDate { get; set; }
        public int UnitId { get; set; }
        public string? UnitName { get; set; }
        public int WarehouseId { get; set; }
        public string? WarehouseName { get; set; }
        public int TotalBags { get; set; }
        public decimal TotalNetWeight { get; set; }
        public string? Remarks { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public string? CreatedByName { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public string? ModifiedByName { get; set; }

        // Detail lines (populated in GetById)
        public List<ProductionPackDetailDto>? ProductionPackDetails { get; set; }
    }
}
