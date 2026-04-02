namespace SalesManagement.Application.ComplaintDepartmentFeedback.Dto
{
    public class FeedbackListDto
    {
        public int? FeedbackId { get; set; }
        public int AssignmentId { get; set; }
        public string? ComplaintNumber { get; set; }
        public DateOnly? ComplaintDate { get; set; }
        public int CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? ProductName { get; set; }
        public int ResponsiblePersonId { get; set; }
        public string? RoleName { get; set; }
        public string? ResponsiblePersonName { get; set; }
        public string? FeedbackStatusName { get; set; }
        public string? SeverityName { get; set; }
        public DateOnly? ExpectedResolutionDate { get; set; }
        public DateTimeOffset? SubmittedDate { get; set; }
        public int ReworkCount { get; set; }
        public bool IsMandatory { get; set; }
    }
}
