using SalesManagement.Domain.Common;

namespace SalesManagement.Domain.Entities
{
    public class ComplaintAttachment : BaseEntity
    {
        // Same-module FK → Sales.ComplaintHeader
        public int ComplaintHeaderId { get; set; }
        public ComplaintHeader? ComplaintHeader { get; set; }

        // File details
        public string? FileName { get; set; }
        public string? FilePath { get; set; }
        public string? FileType { get; set; }
        public long? FileSize { get; set; }
    }
}
