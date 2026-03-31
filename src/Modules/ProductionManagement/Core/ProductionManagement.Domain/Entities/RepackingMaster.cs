using ProductionManagement.Domain.Common;

namespace ProductionManagement.Domain.Entities
{
    public class RepackingMaster : BaseEntity
    {
        // Document
        public int UnitId { get; set; }
        public int ProductionYear { get; set; } = DateTime.Now.Year;
        public string? RepackDocNo { get; set; }
        public DateOnly RepackDate { get; set; }

        // Item
        public int ItemId { get; set; }

        // Source (Old)
        public int OldPackTypeId { get; set; }
        public decimal OldNetWeightPerPack { get; set; }
        public int OldStartPackNo { get; set; }
        public int OldEndPackNo { get; set; }
        public int OldTotalBags { get; set; }
        public decimal OldNetWeight { get; set; }
        public int OldWarehouseId { get; set; }
        public int OldBinId { get; set; }

        // Target (New)
        public int PackTypeId { get; set; }
        public decimal NetWeightPerPack { get; set; }
        public int StartPackNo { get; set; }
        public int EndPackNo { get; set; }
        public int TotalBags { get; set; }
        public decimal NetWeight { get; set; }
        public int WarehouseId { get; set; }
        public int BinId { get; set; }

        // Loose
        public decimal LooseConeKgs { get; set; }
        public int? LooseHandlingId { get; set; }

        // Other
        public string? Remarks { get; set; }

        // Same-module navigations
        public PackType? OldPackType { get; set; }
        public PackType? NewPackType { get; set; }        
        public MiscMaster? LooseHandling { get; set; }
    }
}
