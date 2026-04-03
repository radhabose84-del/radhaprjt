using ProductionManagement.Domain.Common;

namespace ProductionManagement.Domain.Entities
{
    public class RepackingHeader : BaseEntity
    {
        // Document
        public int UnitId { get; set; }
        public int ProductionYear { get; set; } = DateTime.Now.Year;
        public string? RepackDocNo { get; set; }
        public DateOnly RepackDate { get; set; }

        // Target (New)
        public int ItemId { get; set; }
        public int PackTypeId { get; set; }
        public int StartPackNo { get; set; }
        public int EndPackNo { get; set; }
        public decimal NetWeightPerPack { get; set; }
        public int TotalBags { get; set; }
        public decimal NetWeight { get; set; }
        public int WarehouseId { get; set; }
        public int BinId { get; set; }

        // Source (Old) — header-level fields
        public int OldItemId { get; set; }
        public int OldPackTypeId { get; set; }

        // Loose
        public decimal LooseConeKgs { get; set; }
        public int? LooseHandlingId { get; set; }

        // Waste
        public int? FaultId { get; set; }
        public int? WasteTypeId { get; set; }
        public decimal WasteQuantity { get; set; }
        public string? WasteReason { get; set; }

        // Other
        public string? Remarks { get; set; }
        public int? LotId { get; set; }
        public int? TypeId { get; set; }

        // Same-module navigations
        public PackType? OldPackType { get; set; }
        public PackType? NewPackType { get; set; }
        public MiscMaster? LooseHandling { get; set; }
        public MiscMaster? Fault { get; set; }
        public MiscMaster? WasteType { get; set; }
        public LotMaster? Lot { get; set; }
        public MiscMaster? Type { get; set; }

        // Child collection
        public ICollection<RepackingDetail>? RepackingDetails { get; set; }
    }
}
