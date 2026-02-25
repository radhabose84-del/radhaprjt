using FAM.Application.AssetSubCategories.Queries.GetAssetSubCategories;
using MediatR;

namespace FAM.Application.AssetSubCategories.Queries.GetAssetSubCategoriesById
{
    public class GetAssetSubCategoriesByIdQuery: IRequest<AssetSubCategoriesDto>
    {
        public int Id { get; set; }
    }
}