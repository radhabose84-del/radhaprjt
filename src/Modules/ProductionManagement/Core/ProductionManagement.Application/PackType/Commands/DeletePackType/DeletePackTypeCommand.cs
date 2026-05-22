using Contracts.Common;
using MediatR;

namespace ProductionManagement.Application.PackType.Commands.DeletePackType;

public sealed record DeletePackTypeCommand(int Id) : IRequest<bool>, IRequirePermission
{
    public PermissionType RequiredPermission => PermissionType.CanDelete;
}
