using MediatR;
using Contracts.Common;

namespace MaintenanceManagement.Application.ActivityMaster.Command.CreateActivityMaster
{
    public class CreateActivityMasterCommand : IRequest<int>, IRequirePermission
    {

        public CreateActivityMasterDto? CreateActivityMasterDto { get; set; }
        


        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
