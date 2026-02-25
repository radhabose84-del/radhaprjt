using MassTransit;

namespace Contracts.Commands.Hangfire
{
    public class HangfireWorkOrderScheduleCommand : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
        public int SchedulerId { get; set; }
        public int DelayInMinutes { get; set; }
    }
}