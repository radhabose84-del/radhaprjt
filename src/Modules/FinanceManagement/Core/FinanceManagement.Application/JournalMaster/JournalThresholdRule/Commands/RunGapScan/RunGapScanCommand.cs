using Contracts.Common;
using MediatR;

namespace FinanceManagement.Application.JournalMaster.JournalThresholdRule.Commands.RunGapScan
{
    // US-GL01-03B — manual trigger for the voucher-number gap scan (same service the Hangfire job runs).
    // Returns the total number of missing numbers found across all series; writes a SequenceGapScanLog row per series.
    public sealed class RunGapScanCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public PermissionType RequiredPermission => PermissionType.CanView;
    }
}
