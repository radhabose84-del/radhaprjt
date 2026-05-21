using MassTransit;

namespace Contracts.Commands.Workflow
{
    public class CreateApprovalRequestCommand : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
        public string ModuleTypeName { get; set; } = default!;
        public int ModuleTransactionId { get; set; }
        public string Payload { get; set; } = default!;
        public int? TransactionTypeId { get; set; }
    }
}