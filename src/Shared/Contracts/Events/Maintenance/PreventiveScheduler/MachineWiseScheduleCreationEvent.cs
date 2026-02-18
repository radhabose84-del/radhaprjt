using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Dtos.Maintenance.Preventive;
using MassTransit;

namespace Contracts.Events.Maintenance.PreventiveScheduler
{
    public class MachineWiseScheduleCreationEvent : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
        public int PreventiveSchedulerHeaderId { get; set; }
        public ICollection<ScheduleDetailSagaDto> ScheduleDetail { get; set; } = default!;
        // public string token { get; set; }
    }
}