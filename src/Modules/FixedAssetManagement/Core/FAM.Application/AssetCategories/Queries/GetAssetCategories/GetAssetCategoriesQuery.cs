using Contracts.Common;
using MediatR;

namespace FAM.Application.AssetCategories.Queries.GetAssetCategories
{
    public class GetAssetCategoriesQuery : IRequest<ApiResponseDTO<List<AssetCategoriesDto>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public string? SearchTerm { get; set; }
    }
}