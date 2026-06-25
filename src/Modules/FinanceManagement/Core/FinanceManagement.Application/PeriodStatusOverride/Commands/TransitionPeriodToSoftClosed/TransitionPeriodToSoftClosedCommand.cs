using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.PeriodStatusOverride.Commands.TransitionPeriodToSoftClosed
{
    public sealed record TransitionPeriodToSoftClosedCommand(int PeriodId)
        : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
