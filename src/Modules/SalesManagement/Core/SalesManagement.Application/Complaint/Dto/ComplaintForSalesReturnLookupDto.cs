namespace SalesManagement.Application.Complaint.Dto
{
    public sealed class ComplaintForSalesReturnLookupDto
    {
        public int Id { get; set; }
        public string? ComplaintNumber { get; set; }
        public DateOnly ComplaintDate { get; set; }
        public int CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? ReturnStatusName { get; set; }
    }
}
