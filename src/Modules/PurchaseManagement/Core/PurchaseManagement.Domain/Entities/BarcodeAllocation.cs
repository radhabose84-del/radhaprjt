using PurchaseManagement.Domain.Common;

namespace PurchaseManagement.Domain.Entities
{
    public class BarcodeAllocation : BaseEntity
    {
        // Auto-generated unique reference (e.g. BBA-2025-0008) — immutable after create
        public string? AllocationNumber { get; set; }

        public DateTimeOffset AllocationDate { get; set; }

        // Passing person — external HR snapshot (no FK; sourced from dbo.GetEmployeeByDivision_TVP)
        public string? EmployeeNo { get; set; }
        public string? EmployeeName { get; set; }

        // Same-module FK -> Purchase.BarcodeSeries (the source series being allocated from)
        public int BarcodeSeriesId { get; set; }

        public long BarcodeFrom { get; set; }
        public long BarcodeTo { get; set; }

        // Count already inwarded/consumed (future Arrival story); drives Balance/Status
        public int UsedQuantity { get; set; }

        // Same-module FK -> Purchase.MiscMaster (MiscType "BarcodeAllocationStatus", default "Open")
        public int StatusId { get; set; }

        public string? Remarks { get; set; }

        // Same-module navigation properties
        public BarcodeSeries? BarcodeSeries { get; set; }
        public MiscMaster? Status { get; set; }
    }
}
