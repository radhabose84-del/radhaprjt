using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.CoaChangeRequest.Commands.ApproveCoaUnfreeze
{
    // US-GL02-08B (AC1/AC2) — record one approval on an unfreeze request. The caller's role (CFO or
    // System Admin) determines which slot is filled; the two approvers must be DISTINCT users. When both
    // distinct slots are filled the window opens (08A) and alerts go to CFO / FC / Internal Audit.
    public class ApproveCoaUnfreezeCommand : IRequest<ApiResponseDTO<bool>>, IRequirePermission
    {
        public PermissionType RequiredPermission => PermissionType.CanApprove;

        public int UnfreezeRequestId { get; set; }
    }
}
