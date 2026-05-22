using MediatR;
using Contracts.Common;

namespace MaintenanceManagement.Application.ActivityCheckListMaster.Command.CreateActivityCheckListMaster
{
    public class CreateActivityCheckListMasterCommand : IRequest<int>, IRequirePermission
    {

        public int ActivityID { get; set; }
        public string? ActivityCheckList { get; set; }       
        public int  UnitId { get; set; }

        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
