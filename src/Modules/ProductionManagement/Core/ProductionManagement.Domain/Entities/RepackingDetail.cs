namespace ProductionManagement.Domain.Entities
{
    public class RepackingDetail
    {
        public int Id { get; set; }
        public int RepackHeaderId { get; set; }

        // Source (Old) pack range
        public int OldStartPackNo { get; set; }
        public int OldEndPackNo { get; set; }
        public decimal OldNetWeightPerPack { get; set; }
        public int OldTotalBags { get; set; }
        public decimal OldNetWeight { get; set; }
        public int OldWarehouseId { get; set; }
        public int OldBinId { get; set; }

        // Same-module navigation
        public RepackingHeader RepackingHeader { get; set; } = null!;
    }
}
