namespace SalesManagement.Application.ComplaintDepartmentFeedback.Dto
{
    public class SubmitAttachmentDto
    {
        public int? Id { get; set; }
        public string? FileName { get; set; }
        public string? FilePath { get; set; }
        public string? FileType { get; set; }
        public long? FileSize { get; set; }
    }
}
