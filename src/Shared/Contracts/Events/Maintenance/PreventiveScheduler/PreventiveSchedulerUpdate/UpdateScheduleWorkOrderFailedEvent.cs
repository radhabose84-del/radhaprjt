using Contracts.Dtos.Maintenance.Preventive;

namespace Contracts.Events.Maintenance.PreventiveScheduler.PreventiveSchedulerUpdate
{
    public class UpdateScheduleWorkOrderFailedEvent
    {
        public Guid CorrelationId { get; set; }
        public string Reason { get; set; } = default!;
        // public List<MachinedetailDto> MachinedetailDtos { get; set; }
        public RollbackHeaderDto rollbackHeaders { get; set; } = default!;
        public string token { get; set; } = default!;
    }
}