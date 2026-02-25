using MassTransit;

namespace Contracts.Events.Maintenance.PreventiveScheduler.PreventiveSchedulerUpdate
{
    public class UpdateScheduleWorkOrderEvent : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
    }
}