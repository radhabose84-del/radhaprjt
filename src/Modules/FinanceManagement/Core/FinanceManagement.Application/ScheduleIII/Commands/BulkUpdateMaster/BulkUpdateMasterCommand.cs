using Contracts.Common;
using FinanceManagement.Application.ScheduleIII.Dto;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Commands.BulkUpdateMaster
{
    // Updates many lines (ScheduleIIIDetail) in one call — section / line / order / active per row.
    public class BulkUpdateMasterCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public List<BulkDetailLineUpdate> Lines { get; set; } = new();

        public PermissionType RequiredPermission => PermissionType.CanUpdate;
    }
}
