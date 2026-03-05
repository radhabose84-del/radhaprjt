using MediatR;
using SalesManagement.Application.OfficerAgent.Dto;

namespace SalesManagement.Application.OfficerAgent.Queries.GetOfficerAgentAutoComplete
{
    public sealed record GetOfficerAgentAutoCompleteQuery(string? Term)
        : IRequest<IReadOnlyList<OfficerAgentGroupedDto>>;
}
