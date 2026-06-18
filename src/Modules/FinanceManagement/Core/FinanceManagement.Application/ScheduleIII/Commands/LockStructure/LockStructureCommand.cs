using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Commands.LockStructure
{
    // Lock the token company/division structure at go-live; post-lock edits route through FR-008 change control.
    public class LockStructureCommand : IRequest<ApiResponseDTO<bool>>, IRequirePermission
    {
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
