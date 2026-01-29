using MediatR;

namespace MaintenanceManagement.Application.Maintenance.WorkOrder.Command.UpdateWorkOrderRequestDate
{    
    public class UpdateWorkOrderRequestDateCommand : IRequest<bool>
    {
        public int WorkOrderId { get; set; }
        public DateTimeOffset RequestDate { get; set; }
        public int IsSystemTime { get; set; }
    }
}
