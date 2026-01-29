
using FAM.Application.Common.HttpResponse;
using MediatR;

namespace FAM.Application.AssetMaster.AssetMasterGeneral.Queries.GetAssetMasterGeneral
{
    public class GetAssetMasterGeneralQuery : IRequest<ApiResponseDTO<List<AssetMasterGeneralDTO>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; } 
        public string? SearchTerm { get; set; }
    }
}