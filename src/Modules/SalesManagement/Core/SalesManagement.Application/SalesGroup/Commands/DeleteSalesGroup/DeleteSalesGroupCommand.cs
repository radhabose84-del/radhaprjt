using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.SalesGroup.Commands.DeleteSalesGroup
{
    public sealed record DeleteSalesGroupCommand(int Id) : IRequest<bool>, IRequirePermission
{
    public PermissionType RequiredPermission => PermissionType.CanDelete;
}
}
