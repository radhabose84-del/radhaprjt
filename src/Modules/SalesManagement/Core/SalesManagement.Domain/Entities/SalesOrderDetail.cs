namespace SalesManagement.Domain.Entities
{
    public class SalesOrderDetail
    {
        public int Id { get; set; }
        public int SalesOrderHeaderId { get; set; }
        public SalesOrderHeader SalesOrderHeader { get; set; } = null!;

        // Item Details
        public int ItemId { get; set; }                        // Cross-module FK (InventoryManagement)
        public int? VariantId { get; set; }                    // Optional, no lookup yet
        public int HSNId { get; set; }                         // Cross-module FK (InventoryManagement)

        // Quantity & Weight
        public int QtyInBags { get; set; }
        public decimal BagWeight { get; set; }
        public int SaleUOMId { get; set; }                     // Cross-module FK (InventoryManagement)
        public decimal TotalWeight { get; set; }               // QtyInBags * BagWeight

        // Pricing
        public decimal ExMillRate { get; set; }
        public decimal DiscountPerUnit { get; set; }
        public decimal Freight { get; set; }

        // Tax
        public decimal TaxableAmount { get; set; }
        public decimal TaxPercentage { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TCSPercentage { get; set; }
        public decimal TCSAmount { get; set; }

        // Computed
        public decimal NetAmount { get; set; }
        public decimal NetRatePerKg { get; set; }

        // Delivery & Tracking
        public DateOnly ExpectedDeliveryDate { get; set; }
        public decimal AgentCommissionPercentage { get; set; }
        public int DispatchedQty { get; set; }
        public int PendingQty { get; set; }                    // QtyInBags - DispatchedQty

        // Status
        public int? LineItemStatusId { get; set; }             // FK → Sales.MiscMaster
        public MiscMaster? LineItemStatus { get; set; }

        // Reverse navigation (DispatchAdvice)
        public ICollection<DispatchAdviceDetail>? DispatchAdviceDetails { get; set; }
    }
}
