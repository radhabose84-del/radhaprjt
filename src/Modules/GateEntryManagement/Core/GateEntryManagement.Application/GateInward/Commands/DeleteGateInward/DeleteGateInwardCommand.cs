using Contracts.Common;
using MediatR;

namespace GateEntryManagement.Application.GateInward.Commands.DeleteGateInward
{
    public sealed record DeleteGateInwardCommand(int Id) : IRequest<bool>, IRequirePermission
{
    public PermissionType RequiredPermission => PermissionType.CanDelete;
}
}
