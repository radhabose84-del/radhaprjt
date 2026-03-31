namespace ProductionManagement.Application.RepackingMaster.Dto
{
    public class RepackingMasterDto
    {
        public int Id { get; set; }
        public int UnitId { get; set; }
        public string? UnitName { get; set; }
        public int ProductionYear { get; set; }
        public string? RepackDocNo { get; set; }
        public DateOnly RepackDate { get; set; }
        public int ItemId { get; set; }
        public string? ItemName { get; set; }
        public int? SelectionModeId { get; set; }
        public string? SelectionModeName { get; set; }

        // Source (Old)
        public int OldPackTypeId { get; set; }
        public string? OldPackTypeName { get; set; }
        public decimal OldNetWeightPerPack { get; set; }
        public int OldStartPackNo { get; set; }
        public int OldEndPackNo { get; set; }
        public int OldTotalBags { get; set; }
        public decimal OldNetWeight { get; set; }
        public int OldWarehouseId { get; set; }
        public string? OldWarehouseName { get; set; }
        public int OldBinId { get; set; }
        public string? OldBinName { get; set; }

        // Target (New)
        public int PackTypeId { get; set; }
        public string? PackTypeName { get; set; }
        public decimal NetWeightPerPack { get; set; }
        public int StartPackNo { get; set; }
        public int EndPackNo { get; set; }
        public int TotalBags { get; set; }
        public decimal NetWeight { get; set; }
        public int WarehouseId { get; set; }
        public string? WarehouseName { get; set; }
        public int BinId { get; set; }
        public string? BinName { get; set; }

        // Loose
        public decimal LooseConeKgs { get; set; }
        public int? LooseHandlingId { get; set; }
        public string? LooseHandlingName { get; set; }

        // Other
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
