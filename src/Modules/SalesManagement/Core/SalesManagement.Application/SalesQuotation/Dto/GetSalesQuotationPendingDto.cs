namespace SalesManagement.Application.SalesQuotation.Dto
{
    public class GetSalesQuotationPendingDto
    {
        // Header fields
        public int Id { get; set; }
        public string? QuotationNo { get; set; }
        public DateOnly QuotationDate { get; set; }
        public DateOnly ValidityDate { get; set; }
        public int CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public int? SalesEnquiryId { get; set; }
        public int? ContactPersonId { get; set; }
        public string? ContactPersonName { get; set; }
        public int PaymentTermId { get; set; }
        public string? PaymentTermDescription { get; set; }
        public int DeliveryTermId { get; set; }
        public string? DeliveryTermDescription { get; set; }
        public int? StatusId { get; set; }
        public string? StatusName { get; set; }
        public decimal FreightCharges { get; set; }
        public decimal OtherCharges { get; set; }
        public decimal TotalBasicAmount { get; set; }
        public decimal TotalDiscount { get; set; }
        public decimal NetTaxableAmount { get; set; }
        public decimal TotalTax { get; set; }
        public decimal GrandTotal { get; set; }
        public string? Remarks { get; set; }
        public string? CreatedByName { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }

        // Workflow fields
        public int ApproverId { get; set; }
        public string? ApproverName { get; set; }
        public int ApprovalRequestHeaderId { get; set; }
        public byte IsEdit { get; set; }
    }
}
