using MediatR;
using Contracts.Common;

namespace MaintenanceManagement.Application.ActivityMaster.Command.DeleteActivityMaster
{
    public class DeleteActivityMasterCommand : IRequest<bool>, IRequirePermission
    {
         public int Id { get; set; }
         public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
