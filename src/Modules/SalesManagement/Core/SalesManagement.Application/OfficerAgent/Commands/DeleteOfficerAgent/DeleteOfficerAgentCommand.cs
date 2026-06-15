using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.OfficerAgent.Commands.DeleteOfficerAgent
{
    public sealed record DeleteOfficerAgentCommand(int Id) : IRequest<bool>, IRequirePermission
{
    public PermissionType RequiredPermission => PermissionType.CanDelete;
}
}
