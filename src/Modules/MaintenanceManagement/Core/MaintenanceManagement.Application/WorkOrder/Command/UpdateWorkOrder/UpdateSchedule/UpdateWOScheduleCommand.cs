

using Contracts.Common;
using MediatR;

namespace MaintenanceManagement.Application.WorkOrder.Command.UpdateWorkOrder.UpdateSchedule
{
    public class UpdateWOScheduleCommand : IRequest<ApiResponseDTO<bool>>, IRequirePermission     
    {
        public WorkOrderScheduleUpdateDto? WOSchedule { get; set; }    
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
