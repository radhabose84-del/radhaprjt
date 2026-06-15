using Contracts.Common;
using MediatR;

namespace MaintenanceManagement.Application.MachineSpecification.DeleteMachineSpecfication
{
    public class DeleteMachineSpecficationCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public int Id { get; set; } 
        public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
