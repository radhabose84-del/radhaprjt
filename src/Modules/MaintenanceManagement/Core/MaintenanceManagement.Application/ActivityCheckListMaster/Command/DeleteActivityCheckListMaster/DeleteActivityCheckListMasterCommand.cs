using MediatR;
using Contracts.Common;

namespace MaintenanceManagement.Application.ActivityCheckListMaster.Command.DeleteActivityCheckListMaster
{
    public class DeleteActivityCheckListMasterCommand : IRequest<bool>, IRequirePermission 
    {
          public int Id { get; set; }
          public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
