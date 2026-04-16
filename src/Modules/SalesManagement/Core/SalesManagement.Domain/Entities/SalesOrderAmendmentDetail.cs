namespace SalesManagement.Domain.Entities
{
    public class SalesOrderAmendmentDetail
    {
        public int Id { get; set; }
        public int SalesOrderAmendmentHeaderId { get; set; }

        // "Modified" or "Removed"
        public string? ChangeType { get; set; }

        // Always references an existing SalesOrderDetail row
        public int SalesOrderDetailId { get; set; }

        // Old Values — snapshot at time of amendment (always populated)
        public int OldItemId { get; set; }
        public int OldQtyInBags { get; set; }
        public decimal OldExMillRate { get; set; }
        public DateOnly OldExpectedDeliveryDate { get; set; }

        // New Values — populated for Modified, null for Removed
        public int? NewQtyInBags { get; set; }
        public decimal? NewExMillRate { get; set; }
        public DateOnly? NewExpectedDeliveryDate { get; set; }

        // Detail-level Computed Fields (captured at amendment time)
        public decimal TotalWeight { get; set; }
        public decimal DiscountPerUnit { get; set; }
        public decimal TaxableAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TCSAmount { get; set; }
        public decimal NetAmount { get; set; }
        public decimal NetRatePerKg { get; set; }
        public int PendingQty { get; set; }
        public decimal? AgentCommissionPercentage { get; set; }

        // Navigation Properties
        public SalesOrderAmendmentHeader? SalesOrderAmendmentHeader { get; set; }
        public SalesOrderDetail? SalesOrderDetail { get; set; }
    }
}
