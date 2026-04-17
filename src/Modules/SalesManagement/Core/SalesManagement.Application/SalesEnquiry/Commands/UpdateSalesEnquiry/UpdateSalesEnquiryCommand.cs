using MediatR;

namespace SalesManagement.Application.SalesEnquiry.Commands.UpdateSalesEnquiry
{
    public class UpdateSalesEnquiryCommand : IRequest<int>
    {
        public int Id { get; set; }
        public int PartyId { get; set; }
        public DateTimeOffset EnquiryDate { get; set; }
        public string? ContactPerson { get; set; }
        public DateTimeOffset? ExpectedDeliveryDate { get; set; }
        public int? PaymentTermId { get; set; }
        public int? SalesLeadId { get; set; }
        public string? Remarks { get; set; }
        public int IsActive { get; set; }
        public List<UpdateSalesEnquiryDetailDto>? SalesEnquiryDetails { get; set; }

        public class UpdateSalesEnquiryDetailDto
        {
            public int ItemId { get; set; }
            public int? VariantId { get; set; }
            public decimal Quantity { get; set; }
            public decimal? ExmillRate { get; set; }
            public decimal? TargetPrice { get; set; }
            public decimal? Discount { get; set; }
        }
    }
}
