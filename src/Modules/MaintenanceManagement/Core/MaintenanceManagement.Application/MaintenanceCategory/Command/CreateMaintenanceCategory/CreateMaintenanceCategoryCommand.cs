using MediatR;
using Contracts.Common;

namespace MaintenanceManagement.Application.MaintenanceCategory.Command.CreateMaintenanceCategory
{
    public class CreateMaintenanceCategoryCommand :IRequest<int>, IRequirePermission
    {
        public string? CategoryName { get; set; }
        public string? Description { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
