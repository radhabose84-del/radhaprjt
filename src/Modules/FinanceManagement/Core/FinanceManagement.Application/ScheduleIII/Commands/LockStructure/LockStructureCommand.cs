using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Commands.LockStructure
{
    // Lock a structure at go-live (by header id); post-lock edits route through FR-008 change control.
    public class LockStructureCommand : IRequest<ApiResponseDTO<bool>>, IRequirePermission
    {
        public int ScheduleIIIHeaderId { get; set; }

        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
