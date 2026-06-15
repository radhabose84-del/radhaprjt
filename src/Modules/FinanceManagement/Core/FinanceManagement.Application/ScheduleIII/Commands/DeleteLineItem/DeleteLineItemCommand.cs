using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Commands.DeleteLineItem
{
    public sealed record DeleteLineItemCommand(int Id) : IRequest<bool>, IRequirePermission
    {
        public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
