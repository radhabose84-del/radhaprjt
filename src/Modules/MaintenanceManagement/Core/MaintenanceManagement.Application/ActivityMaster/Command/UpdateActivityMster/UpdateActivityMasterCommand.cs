using MediatR;
using Contracts.Common;

namespace MaintenanceManagement.Application.ActivityMaster.Command.UpdateActivityMster
{
    public class UpdateActivityMasterCommand  : IRequest<int>, IRequirePermission
    {

     public UpdateActivityMasterDto? UpdateActivityMaster  { get; set; }
     public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
