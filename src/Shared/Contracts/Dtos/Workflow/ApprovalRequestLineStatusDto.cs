namespace Contracts.Dtos.Workflow
{
    public class ApprovalRequestLineStatusDto
    {
        public int ModuleLineTransactionId { get; set; }
        public int ApprovalRequestLineTransactionId { get; set; }
        public string Status { get; set; } = default!;
    }
}