namespace SalesManagement.Application.SalesQuotation.Dto
{
    public class SalesQuotationAmendmentDetailDto
    {
        public int Id { get; set; }
        public int SalesQuotationAmendmentHeaderId { get; set; }
        public string? ChangeType { get; set; }
        public int SalesQuotationDetailId { get; set; }

        // Old Values (snapshot)
        public int OldItemId { get; set; }
        public string? OldItemName { get; set; }
        public decimal OldQuantity { get; set; }
        public decimal OldExMillRate { get; set; }
        public decimal OldDiscount { get; set; }
        public int OldHSNId { get; set; }
        public string? OldHSNCode { get; set; }
        public decimal OldTaxPercentage { get; set; }

        // New Values (null for Removed)
        public int? NewItemId { get; set; }
        public string? NewItemName { get; set; }
        public decimal? NewQuantity { get; set; }
        public decimal? NewExMillRate { get; set; }
        public decimal? NewDiscount { get; set; }
        public int? NewHSNId { get; set; }
        public string? NewHSNCode { get; set; }
        public decimal? NewTaxPercentage { get; set; }

        // Computed Fields
        public decimal NetRate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TaxAmount { get; set; }

        // Auto-generated remarks describing what changed
        public string? Remarks { get; set; }
    }
}
