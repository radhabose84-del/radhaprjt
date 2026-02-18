using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Contracts.Events.Hangfire.PreventiveScheduler
{
    public class HangfireWorkOrderScheduleFailedEvent
    {
        public Guid CorrelationId { get; set; }
        public int SchedulerId { get; set; }
        public int WorkOrderId { get; set; }
        public string Reason { get; set; } = default!;
    }
}