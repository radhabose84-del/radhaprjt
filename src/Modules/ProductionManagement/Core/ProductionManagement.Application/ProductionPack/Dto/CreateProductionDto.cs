namespace ProductionManagement.Application.ProductionPack.Dto
{
    public class CreateProductionDto
    {
        public DateOnly PackDate { get; set; }
        public int ProductionYear { get; set; }
        public int WarehouseId { get; set; }

        // Item identification
        public int ItemId { get; set; }
        public int? VariantId { get; set; }

        // Location & quality (optional)
        public int? BinId { get; set; }
        public int? QualityStatusId { get; set; }

        // Detail rows (lot-level)
        public List<CreateProductionDetailItem> Details { get; set; } = new();
    }

    public class CreateProductionDetailItem
    {
        public int LotId { get; set; }
        public int? PackTypeId { get; set; }
        public decimal? NetWeightPerPack { get; set; }

        // Type: Loose Cone / Packed (FK to MiscMaster)
        public int? TypeId { get; set; }

        // Pack range (null = loose cone / production-only)
        public int? StartPackNo { get; set; }
        public int? EndPackNo { get; set; }

        // Quantities
        public decimal OpeningLooseKgs { get; set; }
        public decimal TotalProductionKgs { get; set; }
        public int TotalBags { get; set; }
        public decimal TotalNetWeight { get; set; }
        public decimal ProductionKgs { get; set; }
        public decimal LooseConeKgs { get; set; }

        public string? Remarks { get; set; }
    }
}
