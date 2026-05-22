using Contracts.Common;
using MediatR;

namespace InventoryManagement.Application.UsageType.Commands.DeleteUsageType
{
    public sealed record DeleteUsageTypeCommand(int Id) : IRequest<bool>, IRequirePermission
{
    public PermissionType RequiredPermission => PermissionType.CanDelete;
}
}
