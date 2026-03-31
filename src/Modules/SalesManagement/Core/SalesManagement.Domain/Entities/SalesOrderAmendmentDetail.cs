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

        // Navigation Properties
        public SalesOrderAmendmentHeader? SalesOrderAmendmentHeader { get; set; }
        public SalesOrderDetail? SalesOrderDetail { get; set; }
    }
}
