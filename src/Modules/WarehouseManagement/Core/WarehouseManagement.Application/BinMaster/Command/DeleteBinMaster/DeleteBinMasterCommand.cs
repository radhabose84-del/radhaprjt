using MediatR;
using Contracts.Common;

namespace WarehouseManagement.Application.BinMaster.Command.DeleteBinMaster
{
    public class DeleteBinMasterCommand : IRequest<bool>, IRequirePermission
    {
        public int  Id { get; set; }
        
        public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
