using MediatR;
using Contracts.Common;

namespace FAM.Application.AssetSubCategories.Command.CreateAssetSubCategories
{
    public class CreateAssetSubCategoriesCommand :IRequest<int>, IRequirePermission
    {
        //public string? Code { get; set; }
        public string? SubCategoryName { get; set; }
        public string? Description { get; set; }
        public int AssetCategoriesId { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
