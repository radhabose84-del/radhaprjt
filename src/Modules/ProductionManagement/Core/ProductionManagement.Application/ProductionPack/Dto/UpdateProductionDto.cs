namespace ProductionManagement.Application.ProductionPack.Dto
{
    public class UpdateProductionDto
    {
        public int Id { get; set; }
        public DateOnly PackDate { get; set; }
        public int ProductionYear { get; set; }
        public int WarehouseId { get; set; }

        // Item / Lot / Pack identification
        public int ItemId { get; set; }
        public int LotId { get; set; }
        public int PackTypeId { get; set; }
        public decimal NetWeightPerPack { get; set; }

        // Pack range (null = Production-only entry)
        public int? StartPackNo { get; set; }
        public int? EndPackNo { get; set; }

        // Totals
        public int NoOfBags { get; set; }
        public int TotalBags { get; set; }
        public decimal TotalNetWeight { get; set; }
        public decimal ProductionKgs { get; set; }
        public decimal LooseConeKgs { get; set; }

        // Location & quality (optional)
        public int? BinId { get; set; }
        public int? QualityStatusId { get; set; }

        public string? Remarks { get; set; }
        public int IsActive { get; set; }
    }
}
