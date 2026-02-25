using Contracts.Dtos.Maintenance.Preventive;
using MassTransit;

namespace Contracts.Commands.Maintenance.PreventiveScheduler.Update
{
    public class RollBackScheduleWorkOrderCommand : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
        // public int PreventiveSchedulerHeaderId { get; set; }
        public string Reason { get; set; } = default!;
        public RollbackHeaderDto rollbackHeaders { get; set; } = default!;
        // public string token { get; set; }
        // public int PreventiveSchedulerDetailId { get; set; }
        // public int DelayInMinutes { get; set; }
        public ICollection<RollbackScheduleDetailDto> rollbackScheduleDetail { get; set; } = default!;
    }
}