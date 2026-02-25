using MassTransit;

namespace Contracts.Commands.Project
{
    public class RollbackTransactionCommand : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
        public int ModuleTransactionId { get; set; }
        public string Reason { get; set; } = default!;
        public string ModuleTypeName { get; set; } = default!;
    }
}