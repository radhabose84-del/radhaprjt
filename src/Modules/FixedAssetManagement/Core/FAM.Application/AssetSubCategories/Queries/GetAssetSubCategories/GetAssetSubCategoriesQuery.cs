using Contracts.Common;
using MediatR;

namespace FAM.Application.AssetSubCategories.Queries.GetAssetSubCategories
{
    public class GetAssetSubCategoriesQuery : IRequest<ApiResponseDTO<List<AssetSubCategoriesDto>>>
    {
        
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public string? SearchTerm { get; set; }
    }
}