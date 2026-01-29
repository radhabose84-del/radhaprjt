using System;

namespace Contracts.Events.Maintenance
{
    public class NextSchedulerCreationFailedEvent
    {
        public Guid CorrelationId { get; set; }
        /* public int WorkOrderId { get; set; } */
        public string Reason { get; set; }
        public string token { get; set; }
    }
}