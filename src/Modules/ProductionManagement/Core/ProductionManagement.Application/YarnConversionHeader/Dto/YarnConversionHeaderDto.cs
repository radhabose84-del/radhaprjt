namespace ProductionManagement.Application.YarnConversionHeader.Dto
{
    public class YarnConversionHeaderDto
    {
        public int Id { get; set; }
        public int UnitId { get; set; }
        public string? UnitName { get; set; }
        public int ProductionYear { get; set; }
        public string? ConversionDocNo { get; set; }
        public DateOnly ConversionDate { get; set; }

        // Source (Old)
        public int LotId { get; set; }
        public string? LotName { get; set; }
        public int OldItemId { get; set; }
        public string? OldItemName { get; set; }
        public int OldPackTypeId { get; set; }
        public string? OldPackTypeName { get; set; }
        public int OldStartPackNo { get; set; }
        public int OldEndPackNo { get; set; }
        public int OldTotalBags { get; set; }
        public decimal OldNetWeightPerPack { get; set; }
        public decimal OldNetWeight { get; set; }
        public int OldWarehouseId { get; set; }
        public string? OldWarehouseName { get; set; }
        public int OldBinId { get; set; }
        public string? OldBinName { get; set; }
        public int? FaultId { get; set; }
        public string? FaultName { get; set; }

        // Target (New)
        public int ItemId { get; set; }
        public string? ItemName { get; set; }
        public int PackTypeId { get; set; }
        public string? PackTypeName { get; set; }
        public int TotalBags { get; set; }
        public decimal NetWeightPerPack { get; set; }
        public decimal NetWeight { get; set; }
        public int StartPackNo { get; set; }
        public int EndPackNo { get; set; }
        public decimal LooseQty { get; set; }
        public int? LooseHandlingId { get; set; }
        public string? LooseHandlingName { get; set; }
        public int WarehouseId { get; set; }
        public string? WarehouseName { get; set; }
        public int BinId { get; set; }
        public string? BinName { get; set; }

        // Waste
        public int? WasteTypeId { get; set; }
        public string? WasteTypeName { get; set; }
        public decimal WasteQty { get; set; }
        public string? WasteReason { get; set; }

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
