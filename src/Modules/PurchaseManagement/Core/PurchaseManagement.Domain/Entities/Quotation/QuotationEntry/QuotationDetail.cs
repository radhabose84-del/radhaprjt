using PurchaseManagement.Domain.Common;
using PurchaseManagement.Domain.Entities.Quotation.QuotationCompare;


namespace PurchaseManagement.Domain.Entities.Quotation.QuotationEntry
{
    public class QuotationDetail : BaseEntity, IActivityTracked
    {
        public int QuotationHeaderId { get; set; }
        public QuotationHeader Header { get; set; } = default!;
        public int ItemId { get; set; }
        public int HsnId { get; set; }
        public decimal Quantity { get; set; }
        public int UomId { get; set; }
        public int CurrencyId { get; set; }
        public decimal Rate { get; set; }
        public decimal? Discount { get; set; }
        public decimal GstPercent { get; set; }
        public decimal? Warranty { get; set; }
        public decimal? ValidityDays { get; set; }
        public decimal? DeliveryDays { get; set; }        
        public decimal LineSubtotal { get; set; } // (Qty*Rate) - LineDiscount
        public decimal GstAmount { get; set; } // LineSubtotal * Gst%
        public decimal Total { get; set; } // Subtotal + Gst
        public int? DiscountTypeId { get; set; }
        public decimal? PandFCharge { get; set; }  
        public MiscMaster? MiscQuoDiscountType { get; set; }         
        public ICollection<QuotationComparisonDetail> ConfirmedLinesDetails { get; set; } = new List<QuotationComparisonDetail>();   
      
    }
}