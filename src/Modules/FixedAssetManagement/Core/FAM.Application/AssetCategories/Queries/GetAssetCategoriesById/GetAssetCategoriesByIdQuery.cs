using FAM.Application.AssetCategories.Queries.GetAssetCategories;
using MediatR;

namespace FAM.Application.AssetCategories.Queries.GetAssetCategoriesById
{
    public class GetAssetCategoriesByIdQuery : IRequest<AssetCategoriesDto>
    {
        public int Id { get; set; }
    }
}