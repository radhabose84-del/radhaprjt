using MassTransit;

namespace Contracts.Commands.Maintenance
{
    public class RollbackNextScheduleCommand : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
        public int SchedulerId { get; set; }
    }
}