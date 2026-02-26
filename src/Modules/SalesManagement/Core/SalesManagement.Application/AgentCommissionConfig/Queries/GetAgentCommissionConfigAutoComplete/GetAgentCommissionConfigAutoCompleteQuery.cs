using MediatR;
using SalesManagement.Application.AgentCommissionConfig.Dto;

namespace SalesManagement.Application.AgentCommissionConfig.Queries.GetAgentCommissionConfigAutoComplete
{
    public sealed record GetAgentCommissionConfigAutoCompleteQuery(string Term)
        : IRequest<IReadOnlyList<AgentCommissionConfigLookupDto>>;
}
