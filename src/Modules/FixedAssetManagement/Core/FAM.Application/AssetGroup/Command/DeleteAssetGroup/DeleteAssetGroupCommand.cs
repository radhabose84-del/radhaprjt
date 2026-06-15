using MediatR;
using Contracts.Common;

namespace FAM.Application.AssetGroup.Command.DeleteAssetGroup
{
    public class DeleteAssetGroupCommand : IRequest<int>, IRequirePermission
    {
        public int Id { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
