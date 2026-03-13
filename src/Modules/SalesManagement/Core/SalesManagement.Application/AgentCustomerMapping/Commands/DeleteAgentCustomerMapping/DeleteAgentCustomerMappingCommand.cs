using MediatR;

namespace SalesManagement.Application.AgentCustomerMapping.Commands.DeleteAgentCustomerMapping
{
    public sealed record DeleteAgentCustomerMappingCommand(int Id) : IRequest<bool>;
}
