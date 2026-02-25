using MassTransit;

namespace Contracts.Commands.Maintenance.PreventiveScheduler.Update
{
    public class UpdateWorkOrderItemActivityCommand : CorrelatedBy<Guid>
    {
        public Guid CorrelationId { get; set; }
    }
}