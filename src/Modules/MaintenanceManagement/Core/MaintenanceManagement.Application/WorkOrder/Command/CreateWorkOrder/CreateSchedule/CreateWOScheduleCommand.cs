

using Contracts.Common;
using MaintenanceManagement.Application.WorkOrder.Command.UpdateWorkOrder;
using MediatR;

namespace MaintenanceManagement.Application.WorkOrder.Command.CreateWorkOrder.CreateSchedule
{
    public class CreateWOScheduleCommand : IRequest<ApiResponseDTO<bool>>, IRequirePermission     
    {
        public WorkOrderScheduleUpdateDto? WOSchedule { get; set; }    
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
