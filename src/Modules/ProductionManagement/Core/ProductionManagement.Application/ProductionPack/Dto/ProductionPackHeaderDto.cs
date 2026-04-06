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

        // Item / Lot / Pack identification
        public int ItemId { get; set; }
        public string? ItemName { get; set; }
        public int LotId { get; set; }
        public string? LotCode { get; set; }
        public int PackTypeId { get; set; }
        public string? PackTypeName { get; set; }
        public decimal NetWeightPerPack { get; set; }

        // Pack range
        public int? StartPackNo { get; set; }
        public int? EndPackNo { get; set; }

        // Totals
        public int NoOfBags { get; set; }
        public int TotalBags { get; set; }
        public decimal TotalNetWeight { get; set; }
        public decimal ProductionKgs { get; set; }
        public decimal LooseConeKgs { get; set; }

        // Location & quality
        public int? BinId { get; set; }
        public string? BinName { get; set; }
        public int? QualityStatusId { get; set; }
        public string? QualityStatusName { get; set; }

        // Stock closing flag
        public bool StockClosing { get; set; }

        public string? Remarks { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public string? CreatedByName { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public string? ModifiedByName { get; set; }
    }
}
