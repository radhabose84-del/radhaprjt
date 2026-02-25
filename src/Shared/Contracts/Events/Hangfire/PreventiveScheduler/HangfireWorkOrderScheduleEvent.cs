using MassTransit;

namespace Contracts.Events.Hangfire.PreventiveScheduler
{
    public class HangfireWorkOrderScheduleEvent : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
    }
}