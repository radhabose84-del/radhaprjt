using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;

namespace Contracts.Events.Hangfire.PreventiveScheduler
{
    public class HangfireWorkOrderScheduleEvent : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
    }
}