namespace SalesManagement.Domain.Entities
{
    public class ProductionPackDetail
    {
        public int Id { get; set; }
        public int ProductionPackHeaderId { get; set; }
        public int ItemSno { get; set; }
        public int LotId { get; set; }
        public int ItemId { get; set; }
        public int PackTypeId { get; set; }
        public decimal NetWeightPerPack { get; set; }
        public int StartPackNo { get; set; }
        public int EndPackNo { get; set; }
        public int NoOfBags { get; set; }
        public int TotalBags { get; set; }
        public decimal TotalNetWeight { get; set; }
        public int BinId { get; set; }
        public int QualityStatusId { get; set; }
        public string? LineRemarks { get; set; }        

        // Same-module navigation
        public ProductionPackHeader ProductionPackHeader { get; set; } = null!;
        public LotMaster? LotMaster { get; set; }
        public PackType? PackType { get; set; }
        public MiscMaster? QualityStatusMisc { get; set; }
    }
}
