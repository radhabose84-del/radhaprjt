namespace SalesManagement.Application.ComplaintDepartmentFeedback.Dto
{
    public class ComplaintDepartmentFeedbackDto
    {
        public int Id { get; set; }
        public int AssignmentId { get; set; }

        // Complaint Context (Read-Only)
        public int ComplaintHeaderId { get; set; }
        public string? ComplaintNumber { get; set; }
        public DateOnly? ComplaintDate { get; set; }
        public string? CustomerName { get; set; }
        public string? SeverityName { get; set; }
        public DateOnly? ExpectedResolutionDate { get; set; }

        // Assignment Context
        public string? RoleName { get; set; }
        public int ResponsiblePersonId { get; set; }
        public string? ResponsiblePersonName { get; set; }
        public bool IsMandatory { get; set; }

        // Feedback Fields
        public string? RootCauseText { get; set; }
        public int? RootCauseCategoryId { get; set; }
        public string? RootCauseCategoryName { get; set; }
        public string? CorrectiveAction { get; set; }
        public string? PreventiveAction { get; set; }
        public string? Remarks { get; set; }
        public int FeedbackStatusId { get; set; }
        public string? FeedbackStatusName { get; set; }
        public int? SubmittedBy { get; set; }
        public string? SubmittedByName { get; set; }
        public DateTimeOffset? SubmittedDate { get; set; }
        public int ReworkCount { get; set; }
        public string? ReworkReason { get; set; }

        // Attachments
        public List<ComplaintFeedbackAttachmentDto>? Attachments { get; set; }

        // Audit
        public bool IsActive { get; set; }
        public int CreatedBy { get; set; }
        public string? CreatedByName { get; set; }
        public DateTimeOffset? CreatedDate { get; set; }
        public int? ModifiedBy { get; set; }
        public string? ModifiedByName { get; set; }
        public DateTimeOffset? ModifiedDate { get; set; }
    }
}
