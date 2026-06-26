using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Commands.UnlockStructure
{
    // Revert a locked structure back to DRAFT (by header id) so the line-up can be edited again.
    public class UnlockStructureCommand : IRequest<ApiResponseDTO<bool>>, IRequirePermission
    {
        public int ScheduleIIIHeaderId { get; set; }

        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
