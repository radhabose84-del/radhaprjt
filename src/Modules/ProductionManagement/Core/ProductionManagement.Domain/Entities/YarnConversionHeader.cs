using ProductionManagement.Domain.Common;

namespace ProductionManagement.Domain.Entities
{
    public class YarnConversionHeader : BaseEntity
    {
        public int UnitId { get; set; }
        public int ProductionYear { get; set; } = DateTime.Now.Year;
        public string? ConversionDocNo { get; set; }
        public DateOnly ConversionDate { get; set; }

        // Source (Old)
        public int LotId { get; set; }
        public int OldItemId { get; set; }
        public int OldPackTypeId { get; set; }
        public int OldStartPackNo { get; set; }
        public int OldEndPackNo { get; set; }
        public int OldTotalBags { get; set; }
        public decimal OldNetWeightPerPack { get; set; }
        public decimal OldNetWeight { get; set; }
        public int OldWarehouseId { get; set; }
        public int OldBinId { get; set; }

        // Target (New)
        public int ItemId { get; set; }
        public int PackTypeId { get; set; }
        public int TotalBags { get; set; }
        public decimal NetWeightPerPack { get; set; }
        public decimal NetWeight { get; set; }
        public decimal LooseQty { get; set; }
        public int WarehouseId { get; set; }
        public int BinId { get; set; }

        // Waste
        public decimal WasteQty { get; set; }

        // Other
        public string? Remarks { get; set; }
        public int? TypeId { get; set; }

        // Same-module navigations
        public LotMaster? Lot { get; set; }
        public PackType? OldPackType { get; set; }
        public PackType? NewPackType { get; set; }
        public MiscMaster? Type { get; set; }
    }
}
