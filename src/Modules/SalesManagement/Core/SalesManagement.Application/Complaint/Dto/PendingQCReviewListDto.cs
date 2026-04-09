namespace SalesManagement.Application.Complaint.Dto
{
    public class PendingQCReviewListDto
    {
        public int Id { get; set; }
        public int ComplaintHeaderId { get; set; }
        public string? ComplaintNumber { get; set; }
        public DateOnly ComplaintDate { get; set; }
        public int CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? PhysicalVerificationName { get; set; }
        public bool LabVerificationRequired { get; set; }
        public string? SeverityName { get; set; }
        public string? Comments { get; set; }
        public string? CreatedByName { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }

        // Workflow fields
        public int ApproverId { get; set; }
        public string? ApproverName { get; set; }
        public int ApprovalRequestHeaderId { get; set; }
        public byte IsEdit { get; set; }
    }
}
