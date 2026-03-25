namespace SalesManagement.Application.Complaint.Dto
{
    public sealed class ComplaintLookupDto
    {
        public int Id { get; set; }
        public string? ComplaintNumber { get; set; }
        public DateOnly ComplaintDate { get; set; }
        public string? CustomerName { get; set; }
    }
}
