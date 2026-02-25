using Contracts.Common;
using MediatR;

namespace PartyManagement.Application.PartyMaster.Queries.GetPartMaster
{
    public class GetPartMasterQuery : IRequest<ApiResponseDTO<List<GetPartyMasterDto>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public string? SearchTerm { get; set; }
    }
}