namespace SalesManagement.Application.ComplaintDepartmentFeedback.Dto
{
    /// <summary>
    /// One row per assigned department for a complaint, with full RCA content + attachments.
    /// FeedbackId is null when the department hasn't submitted yet — the QC person sees
    /// which assignments are still outstanding alongside the ones already submitted.
    /// </summary>
    public sealed class ComplaintFeedbackFullDto
    {
        // Assignment (always present)
        public int AssignmentId { get; set; }
        public int RoleId { get; set; }
        public string? RoleName { get; set; }
        public int ResponsiblePersonId { get; set; }
        public string? ResponsiblePersonName { get; set; }
        public bool IsMandatory { get; set; }
        public string? AssignmentStatusName { get; set; }

        // Feedback (null if pending)
        public int? FeedbackId { get; set; }
        public string? FeedbackStatusName { get; set; }
        public string? RootCauseText { get; set; }
        public int? RootCauseCategoryId { get; set; }
        public string? RootCauseCategoryName { get; set; }
        public string? CorrectiveAction { get; set; }
        public string? PreventiveAction { get; set; }
        public string? Remarks { get; set; }
        public int? SubmittedBy { get; set; }
        public string? SubmittedByName { get; set; }
        public DateTimeOffset? SubmittedDate { get; set; }
        public int ReworkCount { get; set; }
        public string? ReworkReason { get; set; }

        // Attachments (empty list when no feedback)
        public List<ComplaintFeedbackAttachmentDto> Attachments { get; set; } = new();
    }
}
