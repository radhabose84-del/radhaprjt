using Contracts.Common;
using MediatR;

namespace GateEntryManagement.Application.GatePass.Commands.DeleteGatePass
{
    public sealed record DeleteGatePassCommand(int Id) : IRequest<bool>, IRequirePermission
{
    public PermissionType RequiredPermission => PermissionType.CanDelete;
}
}
