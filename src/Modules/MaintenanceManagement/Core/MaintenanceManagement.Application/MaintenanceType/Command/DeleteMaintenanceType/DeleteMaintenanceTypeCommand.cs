using MediatR;
using Contracts.Common;

namespace MaintenanceManagement.Application.MaintenanceType.Command.DeleteMaintenanceType
{
    public class DeleteMaintenanceTypeCommand : IRequest<int>, IRequirePermission
    {
        public int Id { get; set; } 
        public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
