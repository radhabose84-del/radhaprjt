using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PurchaseManagement.Application.Quotation.QuotationCompare.Queries.GetQuoteComparison
{
    public class QuoteComparisonDto
    {
        public int RfqId { get; set; }
        public string RfqCode { get; set; } = string.Empty;
        public bool IsPosted { get; set; }
        public List<QuoteItemDto> Items { get; set; } = new();
        public class QuoteItemDto
        {
            public int ItemId { get; set; }
            public string ItemName { get; set; } = string.Empty;
            public decimal Quantity { get; set; }
            public int UomId { get; set; }
            public string Uom { get; set; } = string.Empty;
            public List<QuoteSupplierDto> Suppliers { get; set; } = new();

        }

        public class QuoteSupplierDto
        {
            public int QuotationHeaderId { get; set; }
            public int QuotationDetailId { get; set; }
            public int SupplierId { get; set; }
            public string SupplierName { get; set; } = string.Empty;
            public decimal UnitPrice { get; set; }
            public decimal DiscountValue { get; set; }
            public decimal Freight { get; set; }
            public decimal Freight_PerItem { get; set; }
            public decimal GstPercent { get; set; }
            public decimal GstAmount { get; set; }
            public DateTimeOffset ValidTill { get; set; }
            public int DeliveryDays { get; set; }
            public decimal Net { get; set; }
            public decimal LandedUnit { get; set; }
            public decimal Total { get; set; }

            // Flags
            public bool IsSuggested { get; set; }
            public bool IsFastest { get; set; }
            public bool IsExpired { get; set; }
            public bool IsDiscount { get; set; }
            
        }
        // ✅ Add a flat row model for Dapper
        public class QuoteComparisonRow
        {
            public int RfqId { get; set; }
            public string RfqCode { get; set; } = string.Empty;
            public bool IsPosted { get; set; }
            public int ItemId { get; set; }
            public string ItemName { get; set; } = string.Empty;
            public decimal Quantity { get; set; }
            public int UomId { get; set; }
            public string Uom { get; set; } = string.Empty;
            public int QuotationHeaderId { get; set; }
            public int QuotationDetailId { get; set; }
            public int SupplierId { get; set; }
            public string SupplierName { get; set; } = string.Empty;
            public decimal UnitPrice { get; set; }
            public decimal Freight { get; set; }
            public decimal Freight_PerItem { get; set; }
            public decimal DiscountValue { get; set; }
            public decimal GstPercent { get; set; }
            public decimal GstAmount { get; set; }
            public DateTimeOffset ValidTill { get; set; }
            public int DeliveryDays { get; set; }
            public decimal Net { get; set; }
            public decimal LandedUnit { get; set; }
            public decimal Total { get; set; }
           
        }
    }
}