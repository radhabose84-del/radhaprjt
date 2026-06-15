using Contracts.Common;
using MediatR;

namespace MaintenanceManagement.Application.MaintenanceRequest.Command.UpdateMaintenanceRequestStatusCommand
{
    public class UpdateMaintenanceRequestStatusCommand  : IRequest<ApiResponseDTO<bool>>, IRequirePermission
    {
        public int Id { get; set; }
       // public int RequestStatusId { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
