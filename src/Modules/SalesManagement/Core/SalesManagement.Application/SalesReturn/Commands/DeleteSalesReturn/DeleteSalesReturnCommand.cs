using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.SalesReturn.Commands.DeleteSalesReturn
{
    public sealed record DeleteSalesReturnCommand(int Id) : IRequest<bool>, IRequirePermission
{
    public PermissionType RequiredPermission => PermissionType.CanDelete;
}
}
