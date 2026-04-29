namespace SalesManagement.Application.SalesEnquiry.Dto
{
    public sealed class SalesEnquiryLookupDto
    {
        public int Id { get; set; }
        public string? EnquiryNo { get; set; }
        public int PartyId { get; set; }
        public string? PartyName { get; set; }
    }
}
