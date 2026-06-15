using MediatR;
using Contracts.Common;

namespace FAM.Application.AssetCategories.Command.CreateAssetCategories
{
    public class CreateAssetCategoriesCommand : IRequest<int>, IRequirePermission
    {
        //public string? Code { get; set; }
        public string? CategoryName { get; set; }
        public string? Description { get; set; }
        public int AssetGroupId { get; set; }
        //public decimal GroupPercentage { get; set; }       
        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
