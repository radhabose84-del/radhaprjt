using Contracts.Common;
using MediatR;

namespace MaintenanceManagement.Application.ShiftMasters.Commands.DeleteShiftMaster
{
    public class DeleteShiftMasterCommand : IRequest<ApiResponseDTO<bool>>, IRequirePermission
    {
        public int Id { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
