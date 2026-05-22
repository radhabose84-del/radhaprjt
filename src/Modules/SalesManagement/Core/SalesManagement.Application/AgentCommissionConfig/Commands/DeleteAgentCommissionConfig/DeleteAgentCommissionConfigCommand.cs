using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.AgentCommissionConfig.Commands.DeleteAgentCommissionConfig
{
    public sealed record DeleteAgentCommissionConfigCommand(int Id) : IRequest<bool>, IRequirePermission
{
    public PermissionType RequiredPermission => PermissionType.CanDelete;
}
}
