using Contracts.Dtos.Maintenance.Preventive;

namespace Contracts.Events.Maintenance.PreventiveScheduler
{
    public class ScheduleWorkOrderFailedEvent
    {
        public Guid CorrelationId { get; set; }
        public string Reason { get; set; } = default!;
        // public string token { get; set; }
        public ICollection<ScheduleDetailSagaDto> ScheduleDetail { get; set; } = default!;
    }
}