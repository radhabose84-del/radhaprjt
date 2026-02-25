namespace Contracts.Events.Workflow
{
    public class ApprovalRequestFailedEvent
    {
        public Guid CorrelationId { get; set; }
        public string Reason { get; set; } = default!;
        public int ModuleTransactionId { get; set; }
        public string ModuleTypeName { get; set; } = default!;
    }
}