using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;

namespace Contracts.Events.Maintenance.PreventiveScheduler
{
    public class ScheduleWorkOrderCreationEvent : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
    }
}