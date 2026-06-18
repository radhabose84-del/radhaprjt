using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Commands.DeleteSubTotal
{
    public sealed record DeleteSubTotalCommand(int Id) : IRequest<bool>, IRequirePermission
    {
        public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
