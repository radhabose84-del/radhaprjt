namespace SalesManagement.Application.SalesOrder.Dto
{
    public class SalesOrderDetailDto
    {
        public int Id { get; set; }
        public int SalesOrderHeaderId { get; set; }

        // Item Details
        public int ItemId { get; set; }
        public string? ItemName { get; set; }
        public int? VariantId { get; set; }
        public string? VariantName { get; set; }
        public int HSNId { get; set; }
        public string? HSNCode { get; set; }
        public int? PackTypeId { get; set; }
        public string? PackTypeName { get; set; }
        public int? YarnTypeId { get; set; }
        public string? YarnTypeName { get; set; }

        // Quantity & Weight
        public int QtyInBags { get; set; }
        public decimal BagWeight { get; set; }
        public int SaleUOMId { get; set; }
        public string? UOMName { get; set; }
        public decimal TotalWeight { get; set; }

        // Pricing
        public decimal ExMillRate { get; set; }
        public decimal DiscountPerUnit { get; set; }
        public decimal Freight { get; set; }
        public decimal? Handling { get; set; }
        public decimal? Charity { get; set; }

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
        public decimal ReservedQty { get; set; }
        public int PendingQty { get; set; }

        // Status
        public int? LineItemStatusId { get; set; }
        public string? LineItemStatusName { get; set; }
    }
}
