using ProductionManagement.Domain.Common;

namespace ProductionManagement.Domain.Entities
{
    public class ProductionPackDetail : BaseEntity
    {
        public string? PackNo { get; set; }
        public DateOnly PackDate { get; set; }
        public int ProductionYear { get; set; } = DateTime.Now.Year;
        public int UnitId { get; set; }
        public int WarehouseId { get; set; }

        // Item / Lot / Pack identification
        public int ItemId { get; set; }
        public int LotId { get; set; }
        public int? PackTypeId { get; set; }
        public decimal? NetWeightPerPack { get; set; }

        // Pack range (null = Production-only entry, no physical bags)
        public int? StartPackNo { get; set; }
        public int? EndPackNo { get; set; }

        // Totals
        public int TotalBags { get; set; }
        public decimal TotalNetWeight { get; set; }
        public decimal ProductionKgs { get; set; }
        public decimal LooseConeKgs { get; set; }

        // Location & quality
        public int? BinId { get; set; }
        public int? QualityStatusId { get; set; }

        // Stock closing — when true, record is locked (no further edits)
        public bool StockClosing { get; set; }

        public string? Remarks { get; set; }

        // Same-module navigations
        public LotMaster? LotMaster { get; set; }
        public PackType? PackType { get; set; }
        public MiscMaster? QualityStatusMisc { get; set; }
    }
}
