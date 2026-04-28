using SalesManagement.Domain.Common;

namespace SalesManagement.Domain.Entities
{
    public class SalesEnquiryHeader : BaseEntity
    {
        public string? EnquiryNo { get; set; }
        public int PartyId { get; set; }
        public DateTimeOffset EnquiryDate { get; set; }
        public string? ContactPerson { get; set; }
        public DateTimeOffset? ExpectedDeliveryDate { get; set; }
        public int? PaymentTermId { get; set; }
        public string? Remarks { get; set; }

        public int? SalesLeadId { get; set; }

        public SalesLead? SalesLead { get; set; }
        public ICollection<SalesEnquiryDetail>? SalesEnquiryDetails { get; set; }

        // Reverse navigation (SalesOrder)
        public ICollection<SalesOrderHeader>? SalesOrderHeaders { get; set; }
    }
}
