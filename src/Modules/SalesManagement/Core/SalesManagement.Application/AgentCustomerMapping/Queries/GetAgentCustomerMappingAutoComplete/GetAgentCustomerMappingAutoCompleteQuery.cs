using MediatR;
using SalesManagement.Application.AgentCustomerMapping.Dto;

namespace SalesManagement.Application.AgentCustomerMapping.Queries.GetAgentCustomerMappingAutoComplete
{
    public sealed record GetAgentCustomerMappingAutoCompleteQuery(string? Term)
        : IRequest<IReadOnlyList<AgentCustomerMappingLookupDto>>;
}
