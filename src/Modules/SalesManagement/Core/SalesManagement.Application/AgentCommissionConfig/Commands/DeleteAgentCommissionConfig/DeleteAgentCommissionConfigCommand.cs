using MediatR;

namespace SalesManagement.Application.AgentCommissionConfig.Commands.DeleteAgentCommissionConfig
{
    public sealed record DeleteAgentCommissionConfigCommand(int Id) : IRequest<bool>;
}
