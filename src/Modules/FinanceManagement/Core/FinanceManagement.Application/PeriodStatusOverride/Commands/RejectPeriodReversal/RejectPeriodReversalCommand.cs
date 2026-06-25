using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.PeriodStatusOverride.Commands.RejectPeriodReversal
{
    public class RejectPeriodReversalCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public int OverrideId { get; set; }
        public string? RejectionReason { get; set; }

        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
