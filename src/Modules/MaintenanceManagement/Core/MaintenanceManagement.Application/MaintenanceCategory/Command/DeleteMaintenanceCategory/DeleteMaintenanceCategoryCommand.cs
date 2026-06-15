using MediatR;
using Contracts.Common;

namespace MaintenanceManagement.Application.MaintenanceCategory.Command.DeleteMaintenanceCategory
{
    public class DeleteMaintenanceCategoryCommand : IRequest<int>, IRequirePermission 
    {
        public int Id { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
