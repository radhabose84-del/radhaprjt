using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.PeriodStatusOverride.Commands.TransitionPeriodToHardClosed
{
    public sealed record TransitionPeriodToHardClosedCommand(int PeriodId)
        : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
