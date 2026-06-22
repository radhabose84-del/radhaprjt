using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.CoaChangeRequest.Commands.SealCoa
{
    // US-GL02-08B (gap G1) — governed CFO seal of the COA (replaces the TEST/ADMIN set-state hook for
    // freezing). Gated by CanApprove; the handler verifies the caller holds the configured CFO role.
    public class SealCoaCommand : IRequest<ApiResponseDTO<bool>>, IRequirePermission
    {
        public PermissionType RequiredPermission => PermissionType.CanApprove;
    }
}
