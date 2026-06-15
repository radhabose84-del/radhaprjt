using MediatR;
using Contracts.Common;

namespace MaintenanceManagement.Application.Maintenance.WorkOrder.Command.UpdateWorkOrderRequestDate
{    
    public class UpdateWorkOrderRequestDateCommand : IRequest<bool>, IRequirePermission
    {
        public int WorkOrderId { get; set; }
        public DateTimeOffset RequestDate { get; set; }
        public int IsSystemTime { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
