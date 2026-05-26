namespace ProductionManagement.Application.ProductionPack.Dto
{
    public class ProductionPackDetailDto
    {
        public int Id { get; set; }
        public int ProductionPackEntryId { get; set; }

        // Lot / Pack identification
        public int LotId { get; set; }
        public string? LotCode { get; set; }
        public int? PackTypeId { get; set; }
        public string? PackTypeName { get; set; }
        public decimal? NetWeightPerPack { get; set; }

        // Type: Loose Cone / Packed (FK to MiscMaster)
        public int? TypeId { get; set; }
        public string? TypeName { get; set; }

        // Pack range
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
