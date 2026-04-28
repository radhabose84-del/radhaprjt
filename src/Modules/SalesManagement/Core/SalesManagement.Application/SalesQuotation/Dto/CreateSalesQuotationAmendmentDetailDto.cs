namespace SalesManagement.Application.SalesQuotation.Dto
{
    public class CreateSalesQuotationAmendmentDetailDto
    {
        // References the existing SalesQuotationDetail row being changed
        public int SalesQuotationDetailId { get; set; }

        // New Values — provide at least one for "Modified"; leave all null for "Removed"
        // ChangeType is auto-derived: any New* value set → "Modified", all null → "Removed"
        public int? NewItemId { get; set; }
        public decimal? NewQuantity { get; set; }
        public decimal? NewExMillRate { get; set; }
        public decimal? NewDiscount { get; set; }
        public int? NewHSNId { get; set; }
        public decimal? NewTaxPercentage { get; set; }

        // Computed Fields (calculated by client)
        public decimal NetRate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TaxAmount { get; set; }
    }
}
