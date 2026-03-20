namespace SalesManagement.Domain.Entities
{
    public class DeliveryChallanDetail
    {
        public int Id { get; set; }
        public int DeliveryChallanHeaderId { get; set; }

        // STO Detail Reference (same-module FK → Sales.StoDetail)
        public int StoDetailId { get; set; }

        // Item (cross-module FK → InventoryManagement)
        public int ItemId { get; set; }

        // Lot (cross-module FK → ProductionManagement)
        public int LotId { get; set; }

        // Pack Numbers
        public int StartPackNo { get; set; }
        public int EndPackNo { get; set; }

        // Quantity & UOM
        public decimal DispatchQuantity { get; set; }
        public int UOMId { get; set; }

        // Counts
        public int? BagCount { get; set; }
        public int? BaleCount { get; set; }

        // Weights
        public decimal NetWeight { get; set; }
        public decimal? GrossWeight { get; set; }

        // Pricing
        public decimal ExMillRate { get; set; }
        public decimal LineMovementValue { get; set; }

        // Navigation properties (same-module)
        public DeliveryChallanHeader DeliveryChallanHeader { get; set; } = null!;
        public StoDetail? StoDetail { get; set; }
        // LotMaster moved to ProductionManagement — use lookup for name

        // Reverse navigation (StoReceipt)
        public ICollection<StoReceiptDetail>? StoReceiptDetails { get; set; }
    }
}
