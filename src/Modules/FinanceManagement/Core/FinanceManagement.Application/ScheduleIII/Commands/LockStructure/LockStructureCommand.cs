using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Commands.LockStructure
{
    // Version & lock the structure at go-live; post-lock edits route through FR-008 change control.
    public class LockStructureCommand : IRequest<ApiResponseDTO<bool>>, IRequirePermission
    {
        public int StructureId { get; set; }

        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
