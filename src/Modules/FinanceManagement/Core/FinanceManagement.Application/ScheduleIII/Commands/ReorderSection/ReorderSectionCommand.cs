using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Commands.ReorderSection
{
    // Moves a section up (Direction = 1) or down (Direction = -1) within its statement type (BS/PL).
    public class ReorderSectionCommand : IRequest<ApiResponseDTO<bool>>, IRequirePermission
    {
        public int Id { get; set; }          // ScheduleIIISection.Id
        public int Direction { get; set; }   // 1 = up, -1 = down

        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
