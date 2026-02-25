using Contracts.Common;
using MediatR;

namespace PartyManagement.Application.PartyGroup.Queries.GetPartyGroup
{
    public class GetPartyGroupQuery : IRequest<ApiResponseDTO<List<PartyGroupDto>>>
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public string? SearchTerm { get; set; }
    }
}