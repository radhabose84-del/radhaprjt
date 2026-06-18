using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Commands.ReorderDetail
{
    // Moves a line up (Direction = 1) or down (Direction = -1) within its structure.
    public class ReorderDetailCommand : IRequest<ApiResponseDTO<bool>>, IRequirePermission
    {
        public int Id { get; set; }          // ScheduleIIIDetail.Id
        public int Direction { get; set; }   // 1 = up, -1 = down

        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
