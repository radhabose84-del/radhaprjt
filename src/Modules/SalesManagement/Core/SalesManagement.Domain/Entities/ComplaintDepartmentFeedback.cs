using SalesManagement.Domain.Common;

namespace SalesManagement.Domain.Entities
{
    public class ComplaintDepartmentFeedback : BaseEntity
    {
        // Same-module FK → Sales.ComplaintQCReviewAssignment (one feedback per assignment)
        public int AssignmentId { get; set; }
        public ComplaintQCReviewAssignment? Assignment { get; set; }

        // Root Cause Analysis
        public string? RootCauseText { get; set; }

        // Same-module FK → Sales.MiscMaster (RootCauseCategory: Process/Material/Machine/etc.)
        public int? RootCauseCategoryId { get; set; }
        public MiscMaster? RootCauseCategory { get; set; }

        // Corrective & Preventive Actions
        public string? CorrectiveAction { get; set; }
        public string? PreventiveAction { get; set; }

        // Remarks
        public string? Remarks { get; set; }

        // Same-module FK → Sales.MiscMaster (FeedbackStatus: Pending/Submitted/Rework Required)
        public int FeedbackStatusId { get; set; }
        public MiscMaster? FeedbackStatus { get; set; }

        // Submission tracking
        public int? SubmittedBy { get; set; }
        public DateTimeOffset? SubmittedDate { get; set; }

        // Rework tracking
        public int ReworkCount { get; set; }
        public string? ReworkReason { get; set; }

        // Child collection
        public ICollection<ComplaintFeedbackAttachment>? Attachments { get; set; }
    }
}
