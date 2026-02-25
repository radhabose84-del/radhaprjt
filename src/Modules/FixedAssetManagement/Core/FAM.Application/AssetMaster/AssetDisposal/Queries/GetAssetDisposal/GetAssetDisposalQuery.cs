using Contracts.Common;
using MediatR;

namespace FAM.Application.AssetMaster.AssetDisposal.Queries.GetAssetDisposal
{
    public class GetAssetDisposalQuery : IRequest<ApiResponseDTO<List<AssetDisposalDto>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public string? SearchTerm { get; set; }
    }
}