using PurchaseManagement.Domain.Common;

namespace PurchaseManagement.Domain.Entities
{
    public class BarcodeSeries : BaseEntity
    {
        // Auto-generated unique reference (e.g. BCS-2025-0008) — immutable after create
        public string? BarcodeSeriesNumber { get; set; }

        // Same-module FK -> Purchase.MiscMaster (MiscType "BarcodePrefix")
        public int PrefixId { get; set; }

        public long BarcodeStartNumber { get; set; }
        public long BarcodeEndNumber { get; set; }

        public DateTimeOffset GenerationDate { get; set; }

        // Count consumed by the allocation process (future story); drives Balance/Status
        public int AllocatedCount { get; set; }

        // Same-module FK -> Purchase.MiscMaster (MiscType "BarcodeSeriesStatus", default "Open")
        public int StatusId { get; set; }

        public string? Remarks { get; set; }

        // Same-module navigation properties (Purchase.MiscMaster)
        public MiscMaster? Prefix { get; set; }
        public MiscMaster? Status { get; set; }

        // Allocations drawn from this series (Purchase.BarcodeAllocation)
        public ICollection<BarcodeAllocation>? Allocations { get; set; }
    }
}
