using Contracts.Common;
using MediatR;

namespace GateEntryManagement.Application.MiscMaster.Commands.DeleteMiscMaster
{
    public sealed record DeleteMiscMasterCommand(int Id) : IRequest<bool>, IRequirePermission
{
    public PermissionType RequiredPermission => PermissionType.CanDelete;
}
}
