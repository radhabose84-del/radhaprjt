using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.PeriodStatusOverride.Commands.RequestPeriodReversal
{
    public class RequestPeriodReversalCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public int PeriodId { get; set; }

        /// <summary>Code value 'OPEN' or 'SOFTCLOSED' — see PeriodStatusConstants.</summary>
        public string? TargetStatusCode { get; set; }

        public string? RequestedReason { get; set; }

        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
