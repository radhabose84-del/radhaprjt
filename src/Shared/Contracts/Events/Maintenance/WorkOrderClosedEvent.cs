using MassTransit;

namespace Contracts.Events.Maintenance
{
    public class WorkOrderClosedEvent   : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
        public int PreventiveSchedulerDetailId { get; set; }
        public int WorkOrderId { get; set; }
        // public string token { get; set; }
    }
}