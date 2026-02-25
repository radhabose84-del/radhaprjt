using MediatR;

namespace FAM.Application.AssetSubCategories.Command.CreateAssetSubCategories
{
    public class CreateAssetSubCategoriesCommand :IRequest<int>
    {
        //public string? Code { get; set; }
        public string? SubCategoryName { get; set; }
        public string? Description { get; set; }
        public int AssetCategoriesId { get; set; }
    }
}