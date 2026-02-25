namespace Contracts.Dtos.Workflow
{
    public class ApprovalRequestHeaderStatusDto
    {
        public int ModuleTransactionId { get; set; }
        public int ApprovalRequestHeaderTransactionId { get; set; }
        public string Status { get; set; } = default!;
    }
}