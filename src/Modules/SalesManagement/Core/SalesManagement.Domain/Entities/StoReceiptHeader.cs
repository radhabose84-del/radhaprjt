using SalesManagement.Domain.Common;

namespace SalesManagement.Domain.Entities
{
    public class StoReceiptHeader : BaseEntity
    {
        public string? StoReceiptNumber { get; set; }
        public DateOnly StoReceiptDate { get; set; }

        // Delivery Challan Reference (same-module FK → Sales.DeliveryChallanHeader)
        public int DeliveryChallanHeaderId { get; set; }

        // Receiving Plant (cross-module FK → UserManagement.Unit)
        public int ReceivingPlantId { get; set; }

        // Receiving Storage Location (cross-module FK → WarehouseManagement)
        public int ReceivingStorageLocationId { get; set; }

        // Rack (cross-module FK → WarehouseManagement.RackMaster)
        public int? RackId { get; set; }

        // Vehicle Number (auto-fetched from DC for verification)
        public string? VehicleNumber { get; set; }

        public string? Remarks { get; set; }

        // Status (same-module FK → Sales.MiscMaster, MiscType = "StoReceiptLineStatus")
        public int StatusId { get; set; }

        // Navigation properties (same-module FKs only)
        public DeliveryChallanHeader? DeliveryChallanHeader { get; set; }
        public MiscMaster? Status { get; set; }

        // Child collection
        public ICollection<StoReceiptDetail>? StoReceiptDetails { get; set; }
    }
}
