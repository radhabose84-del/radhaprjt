using Contracts.Common;
using MediatR;

namespace FAM.Application.AssetMaster.AssetSpecification.Queries.GetAssetSpecification
{
    public class GetAssetSpecificationQuery : IRequest<ApiResponseDTO<List<AssetSpecificationJsonDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; } 
        public string? SearchTerm { get; set; }
    }

}