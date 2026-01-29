using System;
using MassTransit;

namespace Contracts.Commands.Maintenance
{
    public class RollbackWorkOrderCommand : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
        public int WorkOrderId { get; set; }
        public string Reason { get; set; }
        public int SchedulerId { get; set; }
        // public string token { get; set; }
    }
}