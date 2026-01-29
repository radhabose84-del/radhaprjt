using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MassTransit;

namespace Contracts.Commands.Maintenance.PreventiveScheduler
{
    public class RollBackScheduleWorkOrderCommand : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
        public int PreventiveSchedulerHeaderId { get; set; }
        public string Reason { get; set; }
        public string token { get; set; }
    }
}