using PurchaseManagement.Domain.Entities.Quotation.QuotationEntry;

namespace PurchaseManagement.Domain.Entities.Quotation.QuotationCompare
{
    public class QuotationComparisonDetail
    {
        public int Id { get; set; }
        public int QuotationComparisonHeaderId { get; set; }
        public QuotationComparisonHeader QuotationComparisonHeader { get; set; } = null!;
        public int QuotationHeaderId { get; set; }
        public QuotationHeader QuotationHeader { get; set; } = null!;
        public int QuotationDetailId { get; set; }
        public QuotationDetail QuotationCompareDetail { get; set; } = null!;
        public decimal Net { get; set; }
        public decimal LandedUnit { get; set; }
        public decimal Total { get; set; }
        public bool OverrideStatus { get; set; }
        public string? Remarks { get; set; }

    }
        
}