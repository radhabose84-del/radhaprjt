using MediatR;

namespace PartyManagement.Application.PartyMaster.Queries.GetPartMasterAutoComplete
{
    public class GetPartyMasterAutoCompleteQuery : IRequest<List<GetPartyMasterAutoCompleteDto>>
    {
        public List<int>? PartyTypeIds { get; set; } // For multi-select filter
        public string? SearchPattern { get; set; }
        public int? AgentId { get; set; } // Optional: restrict to customers mapped to this agent
    }
}