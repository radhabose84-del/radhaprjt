using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.SalesOrderTypeMaster.Commands.DeleteSalesOrderTypeMaster
{
    public sealed record DeleteSalesOrderTypeMasterCommand(int Id) : IRequest<bool>, IRequirePermission
{
    public PermissionType RequiredPermission => PermissionType.CanDelete;
}
}
