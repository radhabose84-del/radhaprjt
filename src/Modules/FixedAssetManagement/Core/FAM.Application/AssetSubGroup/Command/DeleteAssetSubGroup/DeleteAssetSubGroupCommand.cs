using MediatR;
using Contracts.Common;

namespace FAM.Application.AssetSubGroup.Command.DeleteAssetSubGroup
{
    public class DeleteAssetSubGroupCommand : IRequest<int>, IRequirePermission
    {
        public int Id { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
