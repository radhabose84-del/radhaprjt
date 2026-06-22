using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.CoaChangeRequest.Commands.ApproveCoaChangeImpact
{
    // US-GL02-08B (AC5) — CFO approves the impact assessment on a change request. Gated by CanApprove
    // (existing permission mechanism); the handler also verifies the caller holds the configured CFO role.
    public class ApproveCoaChangeImpactCommand : IRequest<ApiResponseDTO<bool>>, IRequirePermission
    {
        public PermissionType RequiredPermission => PermissionType.CanApprove;

        public int ChangeRequestId { get; set; }
    }
}
