using MediatR;
using Contracts.Common;

namespace FAM.Application.AssetCategories.Command.DeleteAssetCategories
{
    public class DeleteAssetCategoriesCommand : IRequest<int>, IRequirePermission
    {
        public int Id { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
