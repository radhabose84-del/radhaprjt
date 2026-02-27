namespace SalesManagement.Application.SalesQuotation.Dto
{
    public class SalesQuotationHeaderDto
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public DateOnly QuotationDate { get; set; }
        public int? SalesEnquiryId { get; set; }
        public int? ContactPersonId { get; set; }
        public string? ContactPersonName { get; set; }
        public DateOnly ValidityDate { get; set; }
        public int PaymentTermId { get; set; }
        public string? PaymentTermDescription { get; set; }
        public string? Remarks { get; set; }
        public int DeliveryTermId { get; set; }
        public string? DeliveryTermDescription { get; set; }
        public decimal FreightCharges { get; set; }
        public decimal OtherCharges { get; set; }
        public decimal TotalBasicAmount { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal NetTaxableAmount { get; set; }
        public decimal TotalTax { get; set; }
        public decimal GrandTotal { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public string? CreatedByName { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
        public string? ModifiedByName { get; set; }
        public List<SalesQuotationDetailDto>? SalesQuotationDetails { get; set; }
    }
}
