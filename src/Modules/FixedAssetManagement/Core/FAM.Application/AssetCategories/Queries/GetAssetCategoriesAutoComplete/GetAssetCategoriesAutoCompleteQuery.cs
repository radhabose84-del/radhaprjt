using FAM.Application.AssetCategories.Queries.GetAssetCategories;
using MediatR;

namespace FAM.Application.AssetCategories.Queries.GetAssetCategoriesAutoComplete
{
    public class GetAssetCategoriesAutoCompleteQuery : IRequest<List<AssetCategoriesAutoCompleteDto>>
    {
        public string? SearchPattern { get; set; }
    }
}