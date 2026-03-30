namespace ProductionManagement.Application.ProductionPack.Dto
{
    public class ProductionPackDetailDto
    {
        public int Id { get; set; }
        public int ProductionPackHeaderId { get; set; }

        // Header info (populated in GetByPackRangeAsync)
        public string? PackNo { get; set; }
        public DateOnly? PackDate { get; set; }
        public int? ProductionYear { get; set; }
        public int? UnitId { get; set; }
        public string? UnitName { get; set; }
        public int? WarehouseId { get; set; }
        public string? WarehouseName { get; set; }

        public int ItemSno { get; set; }
        public int LotId { get; set; }
        public string? LotCode { get; set; }
        public int ItemId { get; set; }
        public string? ItemName { get; set; }
        public int PackTypeId { get; set; }
        public string? PackTypeName { get; set; }
        public decimal NetWeightPerPack { get; set; }
        public int StartPackNo { get; set; }
        public int EndPackNo { get; set; }
        public int NoOfBags { get; set; }
        public int TotalBags { get; set; }
        public decimal TotalNetWeight { get; set; }
        public int BinId { get; set; }
        public string? BinName { get; set; }
        public int QualityStatusId { get; set; }
        public string? QualityStatusName { get; set; }
        public string? LineRemarks { get; set; }
    }
}
