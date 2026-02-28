namespace SalesManagement.Application.CustomerVisit.Dto
{
    public sealed class CustomerVisitLookupDto
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public DateTimeOffset VisitDateTime { get; set; }
    }
}
