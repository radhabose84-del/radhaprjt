using Contracts.Common;
using MediatR;

namespace MaintenanceManagement.Application.WorkOrder.Command.UpdateWorkOrder
{
    public class UpdateWorkOrderCommand : IRequest<ApiResponseDTO<bool>>, IRequirePermission     
    {
        public WorkOrderUpdateDto? WorkOrder { get; set; }    
        
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
