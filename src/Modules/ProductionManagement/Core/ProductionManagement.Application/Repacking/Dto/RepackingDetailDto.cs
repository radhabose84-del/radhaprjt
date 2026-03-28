namespace ProductionManagement.Application.Repacking.Dto
{
    public class RepackingDetailDto
    {
        public int Id { get; set; }
        public int RepackingHeaderId { get; set; }
        public int ItemId { get; set; }
        public string? ItemName { get; set; }
        public int LotId { get; set; }
        public string? LotCode { get; set; }
        public int BinId { get; set; }
        public string? BinName { get; set; }
        public int WarehouseId { get; set; }
        public string? WarehouseName { get; set; }
        public int PackTypeId { get; set; }
        public string? PackTypeName { get; set; }
        public int StartPackNo { get; set; }
        public int EndPackNo { get; set; }
        public decimal NetWeightPerPack { get; set; }
        public int TotalBags { get; set; }
        public decimal NetWeight { get; set; }
        public int OldPackDetailId { get; set; }
    }
}
