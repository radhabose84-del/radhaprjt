using PurchaseManagement.Domain.Entities.Quotation.RfqEntry;

namespace PurchaseManagement.Domain.Entities.Quotation.QuotationCompare
{
    public class QuotationComparisonHeader
    {
        public int Id { get; set; }
        public int RfqId { get; set; }
        public RfqMaster Rfq { get; set; } = null!;
        public string? RfqCode { get; set; } 
        public DateTimeOffset ConfirmedDate { get; set; }
        public int CreatedBy { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public string? CreatedByName { get; set; }
        public string? CreatedIP { get; set; }
        public int StatusId { get; set; }
        public MiscMaster? StatusQuotation { get; set; }
         public int? ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public string? ModifiedByName { get; set; }
        public string? ModifiedIP { get; set; }
        public ICollection<QuotationComparisonDetail> QuotationConfirmedDetails { get; set; } = new List<QuotationComparisonDetail>();
    }
}