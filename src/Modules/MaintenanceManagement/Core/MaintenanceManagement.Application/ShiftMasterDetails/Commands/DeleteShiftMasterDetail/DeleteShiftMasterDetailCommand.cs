using Contracts.Common;
using MediatR;

namespace MaintenanceManagement.Application.ShiftMasterDetails.Commands.DeleteShiftMasterDetail
{
    public class DeleteShiftMasterDetailCommand : IRequest<ApiResponseDTO<bool>>, IRequirePermission
    {
        public int Id { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
