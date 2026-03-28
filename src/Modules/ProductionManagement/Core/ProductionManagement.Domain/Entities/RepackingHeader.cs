using ProductionManagement.Domain.Common;

namespace ProductionManagement.Domain.Entities
{
    public class RepackingHeader : BaseEntity
    {
        public int UnitId { get; set; }
        public int ProductionYear { get; set; } = DateTime.Now.Year;
        public string? RepackingNo { get; set; }
        public DateOnly RepackingDate { get; set; }
        public int TotalBags { get; set; }
        public decimal NetWeight { get; set; }
        public decimal LooseConeKgs { get; set; }
        public int OldPackHeaderId { get; set; }
        public int? LooseHandlingId { get; set; }
        public string? Remarks { get; set; }

        // Same-module navigations
        public ProductionPackHeader? OldPackHeader { get; set; }
        public MiscMaster? LooseHandling { get; set; }
        public ICollection<RepackingDetail>? RepackingDetails { get; set; }
    }
}
