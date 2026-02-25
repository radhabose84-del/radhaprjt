using FAM.Application.AssetCategories.Queries.GetAssetCategories;
using MediatR;

namespace FAM.Application.AssetCategories.Queries.GetAssetCategoriesByAssetGroupId
{
    public class GetAssetCategoriesByAssetGroupIdQuery : IRequest<List<AssetCategoriesAutoCompleteDto>>
    {
        public int AssetGroupId { get; set; }
    }
}