namespace ProductionManagement.Domain.Entities
{
    public class RepackingDetail
    {
        public int Id { get; set; }
        public int RepackHeaderId { get; set; }

        // Target (New) pack range for this detail row
        public int StartPackNo { get; set; }
        public int EndPackNo { get; set; }

        // Source (Old) pack range
        public int OldStartPackNo { get; set; }
        public int OldEndPackNo { get; set; }

        // Same-module navigation
        public RepackingHeader RepackingHeader { get; set; } = null!;
    }
}
