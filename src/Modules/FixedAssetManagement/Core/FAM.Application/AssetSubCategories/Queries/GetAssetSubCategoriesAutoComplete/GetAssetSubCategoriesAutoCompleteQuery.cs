using FAM.Application.AssetSubCategories.Queries.GetAssetSubCategories;
using MediatR;

namespace FAM.Application.AssetSubCategories.Queries.GetAssetSubCategoriesAutoComplete
{
    public class GetAssetSubCategoriesAutoCompleteQuery : IRequest<List<AssetSubCategoriesAutoCompleteDto>>
    {
        public string? SearchPattern { get; set; }
    }
}