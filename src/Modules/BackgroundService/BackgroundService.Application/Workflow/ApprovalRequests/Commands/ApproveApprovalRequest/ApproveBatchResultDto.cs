namespace BackgroundService.Application.Workflow.ApprovalRequests.Commands.ApproveApprovalRequest
{
    public class ApproveBatchResultDto
    {
        public int Total { get; set; }
        public int ProcessedCount { get; set; }
        public int ApprovedCount { get; set; }
        public int RejectedCount { get; set; }
        public int FailedCount { get; set; }
        public int SkippedCount { get; set; }

        public List<string> Errors { get; set; } = new();

        /// <summary>EInvoice/IRN results per approved invoice (populated when withInvoice=true).</summary>
        public List<EInvoiceBatchResultItem> EInvoiceResults { get; set; } = new();
    }

    public class EInvoiceBatchResultItem
    {
        public int InvoiceId { get; set; }
        public int EInvoiceHeaderId { get; set; }
        public string? Irn { get; set; }
        public string? AckNo { get; set; }
        public DateTimeOffset? AckDate { get; set; }
        public long? EwbNo { get; set; }
        public string? EwbDate { get; set; }
    }
}
