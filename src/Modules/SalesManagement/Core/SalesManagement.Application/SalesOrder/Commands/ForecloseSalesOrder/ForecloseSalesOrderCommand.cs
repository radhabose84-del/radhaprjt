using Contracts.Common;
using MediatR;

namespace SalesManagement.Application.SalesOrder.Commands.ForecloseSalesOrder
{
    public sealed record ForecloseSalesOrderCommand(int Id) : IRequest<bool>, IRequirePermission
    {
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
