namespace SalesManagement.Application.Complaint.Dto
{
    public class PendingComplaintListDto
    {
        public int Id { get; set; }
        public string? ComplaintNumber { get; set; }
        public DateOnly ComplaintDate { get; set; }
        public int CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public int? StatusId { get; set; }
        public string? StatusName { get; set; }
        public string? Remarks { get; set; }
        public string? CreatedByName { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }

        // Workflow fields
        public int ApproverId { get; set; }
        public string? ApproverName { get; set; }
        public int ApprovalRequestHeaderId { get; set; }
        public byte IsEdit { get; set; }
    }
}
