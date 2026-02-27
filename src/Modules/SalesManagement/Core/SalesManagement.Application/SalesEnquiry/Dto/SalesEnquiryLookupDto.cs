namespace SalesManagement.Application.SalesEnquiry.Dto
{
    public sealed class SalesEnquiryLookupDto
    {
        public int Id { get; set; }
        public string? PartyName { get; set; }
        public DateTimeOffset EnquiryDate { get; set; }
        public int TotalItems { get; set; }
    }
}
