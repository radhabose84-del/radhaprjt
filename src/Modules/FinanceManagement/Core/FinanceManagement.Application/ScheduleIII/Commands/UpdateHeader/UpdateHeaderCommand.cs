using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Commands.UpdateHeader
{
    // Updates the structure header (status + textile split) for the token company/division.
    public class UpdateHeaderCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public int StatusId { get; set; }                // MiscMaster (S3_STATUS)
        public int TextileSplitEnabled { get; set; }     // 0/1

        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
