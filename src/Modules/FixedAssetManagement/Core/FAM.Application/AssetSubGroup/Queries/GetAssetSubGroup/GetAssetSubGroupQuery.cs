using FAM.Application.Common.HttpResponse;
using MediatR;

namespace FAM.Application.AssetSubGroup.Queries.GetAssetSubGroup
{
    public class GetAssetSubGroupQuery : IRequest<ApiResponseDTO<List<AssetSubGroupDto>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public string? SearchTerm { get; set; }
    }
}