using MassTransit;

namespace Contracts.Commands.Maintenance.PreventiveScheduler
{
    public class RollBackScheduleWorkOrderCommand : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
        public int PreventiveSchedulerHeaderId { get; set; }
        public string Reason { get; set; } = default!;
        public string token { get; set; } = default!;
    }
}