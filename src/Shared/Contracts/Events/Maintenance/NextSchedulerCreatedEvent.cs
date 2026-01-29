using System;
using MassTransit;
namespace Contracts.Events.Maintenance
{
    public class NextSchedulerCreatedEvent : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
        public int PreventiveSchedulerDetailId { get; set; }
        public int DelayInMinutes { get; set; }
        public int WorkOrderId { get; set; }
    }
}