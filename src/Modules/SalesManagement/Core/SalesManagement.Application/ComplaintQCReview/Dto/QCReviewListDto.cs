namespace SalesManagement.Application.ComplaintQCReview.Dto
{
    public class QCReviewListDto
    {
        public int Id { get; set; }
        public int ComplaintHeaderId { get; set; }
        public string? ComplaintNumber { get; set; }
        public DateOnly? ComplaintDate { get; set; }
        public string? CustomerName { get; set; }
        public string? ItemName { get; set; }
        public string? PhysicalVerificationName { get; set; }
        public string? ComplaintStatusName { get; set; }
        public string? SeverityName { get; set; }
        public int? ReviewedBy { get; set; }
        public string? ReviewedByName { get; set; }
        public DateTimeOffset? ReviewedDate { get; set; }
        public int CustomerId { get; set; }
        public string? ReviewStatus { get; set; }
    }
}
