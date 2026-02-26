namespace SalesManagement.Application.SalesContact.Dto
{
    public sealed class SalesContactLookupDto
    {
        public int Id { get; set; }
        public string? ContactName { get; set; }
        public string? MobileNumber { get; set; }
        public string? ContactTypeName { get; set; }
    }
}
