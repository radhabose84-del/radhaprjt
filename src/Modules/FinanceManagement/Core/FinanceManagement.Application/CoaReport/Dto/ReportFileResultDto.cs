namespace FinanceManagement.Application.CoaReport.Dto
{
    // US-GL02-15 — file payload returned to the controller for a download (return File(...)).
    public class ReportFileResultDto
    {
        public byte[] Content { get; set; } = Array.Empty<byte>();
        public string FileName { get; set; } = "report.pdf";
        public string ContentType { get; set; } = "application/pdf";
    }
}
