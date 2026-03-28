using SalesManagement.Domain.Common;

namespace SalesManagement.Domain.Entities
{
    public class ComplaintFeedbackAttachment : BaseEntity
    {
        // Same-module FK → Sales.ComplaintDepartmentFeedback
        public int FeedbackId { get; set; }
        public ComplaintDepartmentFeedback? Feedback { get; set; }

        // File details
        public string? FileName { get; set; }
        public string? FilePath { get; set; }
        public string? FileType { get; set; }
        public long? FileSize { get; set; }
    }
}
