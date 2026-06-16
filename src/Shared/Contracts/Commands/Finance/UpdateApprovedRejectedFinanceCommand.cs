using MassTransit;

namespace Contracts.Commands.Finance
{
    // Dispatched by BackgroundService's ApprovalResultDispatcherConsumer when a Finance-owned
    // approval (e.g. Account Group Move) is approved/rejected. Consumed by FinanceManagement's
    // ApprovedRejectedConsumer.
    public class UpdateApprovedRejectedFinanceCommand : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
        public int ModuleTransactionId { get; set; }
        public string ModuleTypeName { get; set; } = default!;
        public string Status { get; set; } = default!;
        public int ModifiedBy { get; set; }
        public string? ModifiedByName { get; set; }
        public string? ModifiedIP { get; set; }
    }
}
