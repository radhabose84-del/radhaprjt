using MassTransit;

namespace Contracts.Events.Maintenance.PreventiveScheduler
{
    public class ScheduleWorkOrderCreationEvent : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
    }
}