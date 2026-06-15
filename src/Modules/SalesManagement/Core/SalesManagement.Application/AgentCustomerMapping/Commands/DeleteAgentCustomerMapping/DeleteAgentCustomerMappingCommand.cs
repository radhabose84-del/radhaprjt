using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.AgentCustomerMapping.Commands.DeleteAgentCustomerMapping
{
    public sealed record DeleteAgentCustomerMappingCommand(int Id) : IRequest<bool>, IRequirePermission
{
    public PermissionType RequiredPermission => PermissionType.CanDelete;
}
}
