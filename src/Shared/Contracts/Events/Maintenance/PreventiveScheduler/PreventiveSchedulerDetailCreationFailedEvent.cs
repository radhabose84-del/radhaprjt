using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Contracts.Events.Maintenance.PreventiveScheduler
{
    public class PreventiveSchedulerDetailCreationFailedEvent
    {
        public Guid CorrelationId { get; set; }
        public string Reason { get; set; }
        public string token { get; set; }
    }
}