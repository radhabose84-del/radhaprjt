using MediatR;
using Contracts.Common;

namespace FAM.Application.AssetSubCategories.Command.DeleteAssetSubCategories
{
    public class DeleteAssetSubCategoriesCommand :IRequest<int>, IRequirePermission
    {
         public int Id { get; set; }
         public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
