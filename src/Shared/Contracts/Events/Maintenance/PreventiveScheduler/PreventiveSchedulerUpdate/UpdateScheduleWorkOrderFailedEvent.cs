using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Dtos.Maintenance.Preventive;

namespace Contracts.Events.Maintenance.PreventiveScheduler.PreventiveSchedulerUpdate
{
    public class UpdateScheduleWorkOrderFailedEvent
    {
        public Guid CorrelationId { get; set; }
        public string Reason { get; set; }
        // public List<MachinedetailDto> MachinedetailDtos { get; set; }
        public RollbackHeaderDto rollbackHeaders { get; set; }
        public string token { get; set; }
    }
}