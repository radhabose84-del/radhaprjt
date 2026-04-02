namespace SalesManagement.Application.Complaint.Dto
{
    public class ComplaintAttachmentDto
    {
        public int Id { get; set; }
        public int ComplaintHeaderId { get; set; }
        public string? FileName { get; set; }
        public string? FilePath { get; set; }
        public string? FileType { get; set; }
        public long? FileSize { get; set; }
    }
}
