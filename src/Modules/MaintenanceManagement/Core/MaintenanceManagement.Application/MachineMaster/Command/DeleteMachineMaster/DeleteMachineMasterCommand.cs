using MediatR;
using Contracts.Common;

namespace MaintenanceManagement.Application.MachineMaster.Command.DeleteMachineMaster
{
    public class DeleteMachineMasterCommand : IRequest<bool>, IRequirePermission
    {
        public int Id { get; set; } 
        public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
