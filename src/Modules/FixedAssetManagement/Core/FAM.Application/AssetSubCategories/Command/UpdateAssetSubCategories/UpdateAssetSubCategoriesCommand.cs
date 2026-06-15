using MediatR;
using Contracts.Common;

namespace FAM.Application.AssetSubCategories.Command.UpdateAssetSubCategories
{
    public class UpdateAssetSubCategoriesCommand:IRequest<int>, IRequirePermission
    {
        public int Id { get; set; }
        public string? SubCategoryName { get; set; }
        public string? Description { get; set; }
        public int SortOrder { get; set; }
        public int AssetCategoriesId { get; set; }
        public byte IsActive { get; set; }
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
