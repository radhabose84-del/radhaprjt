using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.PeriodStatusOverride.Commands.ApprovePeriodReversal
{
    public class ApprovePeriodReversalCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public int OverrideId { get; set; }

        /// <summary>"CFO" or "SysAdmin" — which approver-side is acting.</summary>
        public string? Role { get; set; }

        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
