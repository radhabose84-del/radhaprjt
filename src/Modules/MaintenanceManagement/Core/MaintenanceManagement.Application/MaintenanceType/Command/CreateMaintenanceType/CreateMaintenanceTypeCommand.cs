using MediatR;
using Contracts.Common;

namespace MaintenanceManagement.Application.MaintenanceType.Command.CreateMaintenanceType
{
    public class CreateMaintenanceTypeCommand :IRequest<int>, IRequirePermission
    {
         public string? TypeName { get; set; }
         public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
