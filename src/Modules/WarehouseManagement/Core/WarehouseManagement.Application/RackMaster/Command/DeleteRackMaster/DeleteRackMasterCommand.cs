using MediatR;
using Contracts.Common;

namespace WarehouseManagement.Application.RackMaster.Command.DeleteRackMaster
{
    public class DeleteRackMasterCommand : IRequest<bool>, IRequirePermission
    {
        public int  Id { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
