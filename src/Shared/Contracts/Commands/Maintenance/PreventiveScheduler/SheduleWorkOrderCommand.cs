using Contracts.Dtos.Maintenance.Preventive;
using MassTransit;

namespace Contracts.Commands.Maintenance.PreventiveScheduler
{
    public class SheduleWorkOrderCommand : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
        public int PreventiveSchedulerHeaderId { get; set; }
        // public string token { get; set; }
        public ICollection<ScheduleDetailSagaDto> ScheduleDetail { get; set; } = default!;
    }
}