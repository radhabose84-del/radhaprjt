using Contracts.Common;
using MediatR;

namespace ProductionManagement.Application.RawMaterialType.Commands.DeleteRawMaterialType
{
    public sealed record DeleteRawMaterialTypeCommand(int Id) : IRequest<bool>, IRequirePermission
{
    public PermissionType RequiredPermission => PermissionType.CanDelete;
}
}
