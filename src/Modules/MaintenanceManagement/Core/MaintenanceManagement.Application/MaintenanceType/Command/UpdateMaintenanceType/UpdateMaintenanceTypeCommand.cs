using MediatR;
using Contracts.Common;

namespace MaintenanceManagement.Application.MaintenanceType.Command.UpdateMaintenanceType
{
    public class UpdateMaintenanceTypeCommand :IRequest<int>, IRequirePermission
    {
        public int Id {get;set;}
        public string? TypeName { get; set; }
        public byte IsActive { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
