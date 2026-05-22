using Contracts.Common;
using MediatR;

namespace MaintenanceManagement.Application.WorkCenter.Command.DeleteWorkCenter
{
    public class DeleteWorkCenterCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission 
    {
        public int Id { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
