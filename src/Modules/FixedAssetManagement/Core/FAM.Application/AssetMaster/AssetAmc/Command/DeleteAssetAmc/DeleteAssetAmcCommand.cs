using MediatR;
using Contracts.Common;

namespace FAM.Application.AssetMaster.AssetAmc.Command.DeleteAssetAmc
{
    public class DeleteAssetAmcCommand : IRequest<int>, IRequirePermission
    {
        public int Id { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
