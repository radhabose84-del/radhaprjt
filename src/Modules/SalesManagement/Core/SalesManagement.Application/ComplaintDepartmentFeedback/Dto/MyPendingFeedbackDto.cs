namespace SalesManagement.Application.ComplaintDepartmentFeedback.Dto
{
    public class MyPendingFeedbackDto
    {
        public int AssignmentId { get; set; }
        public int? FeedbackId { get; set; }
        public string? ComplaintNumber { get; set; }
        public int CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? ProductName { get; set; }
        public string? ComplaintDescription { get; set; }
        public string? SeverityName { get; set; }
        public DateOnly? ExpectedResolutionDate { get; set; }
        public string? RoleName { get; set; }
        public string? FeedbackStatus { get; set; }
        public string? ReworkReason { get; set; }
    }
}
