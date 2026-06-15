
using Contracts.Common;
using MediatR;

namespace MaintenanceManagement.Application.WorkOrder.Command.CreateWorkOrder
{
    public class CreateWorkOrderCommand : IRequest<ApiResponseDTO<WorkOrderCombineDto>>, IRequirePermission  
    {
       public WorkOrderCombineDto? WorkOrderDto { get; set; }       
       public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
