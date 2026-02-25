using PartyManagement.Application.PartyGroup.Queries.GetPartyGroupAutoComplete;
using MediatR;

namespace PartyManagement.Application.PartyGroup.Queries.GetChildPartyGroupAutoComplete
{
    public class GetChildPartyGroupAutoCompleteQuery : IRequest<List<PartyGroupAutoCompleteDto>>
    {
        public string? SearchPattern { get; set; }
    }
}