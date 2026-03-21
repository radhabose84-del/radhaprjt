namespace SalesManagement.Domain.Entities
{
    public class StoReceiptDetail
    {
        public int Id { get; set; }
        public int StoReceiptHeaderId { get; set; }

        // Delivery Challan Detail Reference (same-module FK)
        public int DeliveryChallanDetailId { get; set; }

        // Item (cross-module FK → InventoryManagement)
        public int ItemId { get; set; }

        // Lot (cross-module FK → ProductionManagement)
        public int LotId { get; set; }

        // Pack Numbers (auto-fetched from DC detail)
        public int StartPackNo { get; set; }
        public int EndPackNo { get; set; }

        // Quantities
        public decimal DispatchQuantity { get; set; }
        public decimal ReceivedQuantity { get; set; }
        public decimal DamageQuantity { get; set; }
        public decimal AcceptedQuantity { get; set; }

        // UOM (cross-module FK → UserManagement.UOM)
        public int UOMId { get; set; }

        // Counts
        public int? BagCount { get; set; }

        // Weights
        public decimal NetWeight { get; set; }
        public decimal? GrossWeight { get; set; }

        // Line Status (same-module FK → Sales.MiscMaster, MiscType = "StoReceiptLineStatus")
        public int LineStatusId { get; set; }

        // Navigation properties (same-module)
        public StoReceiptHeader StoReceiptHeader { get; set; } = null!;
        public DeliveryChallanDetail? DeliveryChallanDetail { get; set; }
        // LotMaster moved to ProductionManagement — use lookup for name
        public MiscMaster? LineStatus { get; set; }
    }
}
