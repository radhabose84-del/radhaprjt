using Contracts.Common;
using MediatR;

namespace FAM.Application.AssetMaster.AssetLocation.Queries.GetCustodian
{
    public class GetCustodianQuery : IRequest<ApiResponseDTO<List<GetCustodianDto>>>
    {
        public string? OldUnitId { get; set; }
        public string? SearchEmployee { get; set; }
        
    }
}