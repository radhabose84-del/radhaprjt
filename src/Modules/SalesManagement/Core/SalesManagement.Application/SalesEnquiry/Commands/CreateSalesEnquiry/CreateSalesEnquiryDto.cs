namespace SalesManagement.Application.SalesEnquiry.Commands.CreateSalesEnquiry
{
    public class CreateSalesEnquiryDto
    {
        public int PartyId { get; set; }
        public DateTimeOffset EnquiryDate { get; set; }
        public string? ContactPerson { get; set; }
        public DateTimeOffset? ExpectedDeliveryDate { get; set; }
        public int? PaymentTermId { get; set; }
        public int? SalesLeadId { get; set; }
        public string? Remarks { get; set; }
        public List<CreateSalesEnquiryDetailDto>? SalesEnquiryDetails { get; set; }

        public class CreateSalesEnquiryDetailDto
        {
            public int ItemId { get; set; }
            public decimal Quantity { get; set; }
            public decimal? ExmillRate { get; set; }
            public decimal? TargetPrice { get; set; }
            public decimal? Discount { get; set; }
        }
    }
}
