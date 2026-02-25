using FAM.Application.AssetSubCategories.Queries.GetAssetSubCategories;
using MediatR;

namespace FAM.Application.AssetSubCategories.Queries.GetAssetSubCategoriesByCategoryId
{
    public class GetAssetSubCategoriesByCategoryIdQuery : IRequest<List<AssetSubCategoriesAutoCompleteDto>>
    {
        public int AssetCategoriesId { get; set; }
    }
        
    
}