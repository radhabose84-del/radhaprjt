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
    }
}
