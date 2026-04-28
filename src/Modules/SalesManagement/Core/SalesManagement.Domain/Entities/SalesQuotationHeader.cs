using SalesManagement.Domain.Common;

namespace SalesManagement.Domain.Entities
{
    public class SalesQuotationHeader : BaseEntity
    {
        public string? QuotationNo { get; set; }
        public int CustomerId { get; set; }
        public DateOnly QuotationDate { get; set; }
        public int? SalesEnquiryId { get; set; }
        public int? ContactPersonId { get; set; }
        public DateOnly ValidityDate { get; set; }
        public int PaymentTermId { get; set; }
        public string? Remarks { get; set; }
        public int DeliveryTermId { get; set; }
        public decimal FreightCharges { get; set; }
        public decimal OtherCharges { get; set; }
        public decimal TotalBasicAmount { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal NetTaxableAmount { get; set; }
        public decimal TotalTax { get; set; }
        public decimal GrandTotal { get; set; }
        public int? StatusId { get; set; }                // FK → Sales.MiscMaster

        // Same-module navigation properties
        public MiscMaster? StatusMisc { get; set; }
        public SalesEnquiryHeader? SalesEnquiryHeader { get; set; }
        public SalesContact? ContactPerson { get; set; }
        public MiscMaster? DeliveryTerm { get; set; }

        // Child collection
        public ICollection<SalesQuotationDetail>? SalesQuotationDetails { get; set; }

        // Reverse navigation from SalesOrderHeader
        public ICollection<SalesOrderHeader>? SalesOrderHeaders { get; set; }
    }
}
