using Contracts.Common;
using MediatR;

namespace ProductionManagement.Application.YarnType.Commands.DeleteYarnType
{
    public sealed record DeleteYarnTypeCommand(int Id) : IRequest<bool>, IRequirePermission
{
    public PermissionType RequiredPermission => PermissionType.CanDelete;
}
}
