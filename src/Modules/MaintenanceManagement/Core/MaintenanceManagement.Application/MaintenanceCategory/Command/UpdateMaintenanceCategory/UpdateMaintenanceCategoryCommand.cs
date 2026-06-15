using MediatR;
using Contracts.Common;

namespace MaintenanceManagement.Application.MaintenanceCategory.Command.UpdateMaintenanceCategory
{
    public class UpdateMaintenanceCategoryCommand  :IRequest<int>, IRequirePermission
    {
        public int Id {get;set;}
        public string? CategoryName { get; set; }
        public string? Description { get; set; }

        public byte IsActive { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
