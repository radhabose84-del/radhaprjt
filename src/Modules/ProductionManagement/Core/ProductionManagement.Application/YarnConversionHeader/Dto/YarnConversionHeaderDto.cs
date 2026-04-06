namespace ProductionManagement.Application.YarnConversionHeader.Dto
{
    public class YarnConversionHeaderDto
    {
        public int Id { get; set; }
        public string? ConversionDocNo { get; set; }
        public DateOnly ConversionDate { get; set; }
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
        public int ItemId { get; set; }
        public string? ItemName { get; set; }
        public int PackTypeId { get; set; }
        public int TotalBags { get; set; }
        public decimal NetWeightPerPack { get; set; }
        public decimal NetWeight { get; set; }
        public decimal LooseQty { get; set; }
        public int WarehouseId { get; set; }
        public int BinId { get; set; }
        public decimal WasteQty { get; set; }
        public string? Remarks { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
    }
}
