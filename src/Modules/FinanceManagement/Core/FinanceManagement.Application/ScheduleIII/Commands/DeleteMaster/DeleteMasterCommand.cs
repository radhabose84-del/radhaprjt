using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Commands.DeleteMaster
{
    // Removes one included line (ScheduleIIIDetail). Id is the detail row id.
    public sealed record DeleteMasterCommand(int Id) : IRequest<bool>, IRequirePermission
    {
        public PermissionType RequiredPermission => PermissionType.CanDelete;
    }
}
