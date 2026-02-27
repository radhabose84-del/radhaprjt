namespace SalesManagement.Application.SalesQuotation.Dto
{
    public class CreateSalesQuotationDto
    {
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
        public List<CreateSalesQuotationDetailDto>? SalesQuotationDetails { get; set; }
    }
}
