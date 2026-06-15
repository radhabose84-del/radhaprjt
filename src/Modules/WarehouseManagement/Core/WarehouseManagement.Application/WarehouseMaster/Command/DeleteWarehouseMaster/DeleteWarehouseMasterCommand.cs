using MediatR;
using Contracts.Common;

namespace WarehouseManagement.Application.WarehouseMaster.Command.DeleteWarehouseMaster
{
    public class DeleteWarehouseMasterCommand  : IRequest<bool>, IRequirePermission
    {
        public int  Id { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
