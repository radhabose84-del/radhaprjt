using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Dtos.Maintenance.Preventive;
using MassTransit;

namespace Contracts.Events.Maintenance.PreventiveScheduler.PreventiveSchedulerUpdate
{
    public class HeaderUpdateEvent : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
        // public int PreventiveSchedulerHeaderId { get; set; }
        // public List<MachinedetailDto> MachinedetailDtos { get; set; }
        // public int UnitId { get; set; }
        // public int FrequencyUnitId { get; set; }
        // public int FrequencyInterval { get; set; }
        // public int ReminderWorkOrderDays { get; set; }
        // public int ReminderMaterialReqDays { get; set; }
        public RollbackHeaderDto rollbackHeaders { get; set; } = default!;
        // public string token { get; set; }
        // public bool isFrequencyChanged { get; set; }
        public ICollection<RollbackScheduleDetailDto> rollbackDetails { get; set; } = default!;
        // public int PreventiveSchedulerDetailId { get; set; }
        // public int DelayInMinutes { get; set; }
        public ICollection<ScheduleDetailUpdateDto> ScheduleDetailUpdate { get; set; } = default!;
    }
}