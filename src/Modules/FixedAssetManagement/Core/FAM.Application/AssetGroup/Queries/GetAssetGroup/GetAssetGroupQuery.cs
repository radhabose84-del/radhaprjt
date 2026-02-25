using Contracts.Common;
using MediatR;

namespace FAM.Application.AssetGroup.Queries.GetAssetGroup
{
    public class GetAssetGroupQuery : IRequest<ApiResponseDTO<List<AssetGroupDto>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public string? SearchTerm { get; set; }
    }
}