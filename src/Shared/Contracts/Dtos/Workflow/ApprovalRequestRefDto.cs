namespace Contracts.Dtos.Workflow
{
    /// <summary>
    /// Lightweight reference to a workflow ApprovalRequest row, used by source modules to surface
    /// the approval header id alongside their own records (e.g. the Freight RFQ pending list).
    /// </summary>
    public sealed class ApprovalRequestRefDto
    {
        public int ApprovalRequestId { get; set; }
        public int ModuleTransactionId { get; set; }
        public int StatusId { get; set; }
        public string? ApproverValue { get; set; }
    }
}
