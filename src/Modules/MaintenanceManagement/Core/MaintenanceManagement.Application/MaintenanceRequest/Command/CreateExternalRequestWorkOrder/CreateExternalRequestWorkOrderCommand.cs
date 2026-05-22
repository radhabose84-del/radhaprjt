using Contracts.Common;
using MediatR;

namespace MaintenanceManagement.Application.MaintenanceRequest.Command.CreateExternalRequestWorkOrder
{
    public class CreateExternalRequestWorkOrderCommand  : IRequest<ApiResponseDTO<List<int>>>, IRequirePermission
    {
    public List<int>? Ids { get; set; } 
      

        
    public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
