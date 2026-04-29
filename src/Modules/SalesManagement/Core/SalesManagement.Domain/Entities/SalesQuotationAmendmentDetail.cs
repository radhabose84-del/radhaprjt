namespace SalesManagement.Domain.Entities
{
    public class SalesQuotationAmendmentDetail
    {
        public int Id { get; set; }
        public int SalesQuotationAmendmentHeaderId { get; set; }

        // "Modified" or "Removed"
        public string? ChangeType { get; set; }

        // Historical reference to SalesQuotationDetail (NO DB FK — detail can be physically deleted)
        public int SalesQuotationDetailId { get; set; }

        // Old Values — snapshot at time of amendment (always populated)
        public int OldItemId { get; set; }
        public decimal OldQuantity { get; set; }
        public decimal OldExMillRate { get; set; }
        public decimal OldDiscount { get; set; }
        public int OldHSNId { get; set; }
        public decimal OldTaxPercentage { get; set; }

        // New Values — populated for Modified, null for Removed
        public int? NewItemId { get; set; }
        public decimal? NewQuantity { get; set; }
        public decimal? NewExMillRate { get; set; }
        public decimal? NewDiscount { get; set; }
        public int? NewHSNId { get; set; }
        public decimal? NewTaxPercentage { get; set; }

        // Computed Fields (captured at amendment time)
        public decimal NetRate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TaxAmount { get; set; }

        // Navigation Properties
        public SalesQuotationAmendmentHeader? SalesQuotationAmendmentHeader { get; set; }
    }
}
