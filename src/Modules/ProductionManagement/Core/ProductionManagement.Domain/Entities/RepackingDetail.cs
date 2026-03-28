namespace ProductionManagement.Domain.Entities
{
    public class RepackingDetail
    {
        public int Id { get; set; }
        public int RepackingHeaderId { get; set; }
        public int ItemId { get; set; }
        public int LotId { get; set; }
        public int BinId { get; set; }
        public int WarehouseId { get; set; }
        public int PackTypeId { get; set; }
        public int StartPackNo { get; set; }
        public int EndPackNo { get; set; }
        public decimal NetWeightPerPack { get; set; }
        public int TotalBags { get; set; }
        public decimal NetWeight { get; set; }
        public int OldPackDetailId { get; set; }

        // Same-module navigations
        public RepackingHeader RepackingHeader { get; set; } = null!;
        public LotMaster? LotMaster { get; set; }
        public PackType? PackType { get; set; }
        public ProductionPackDetail? OldPackDetail { get; set; }
    }
}
