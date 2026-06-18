using Contracts.Common;
using FinanceManagement.Application.ScheduleIII.Dto;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Commands.BulkCreateMaster
{
    // Adds many lines (ScheduleIIIDetail) to the token company/division structure in one call.
    // Header is auto-created (DRAFT) on the first line.
    public class BulkCreateMasterCommand : IRequest<ApiResponseDTO<int>>, IRequirePermission
    {
        public List<BulkDetailLineInput> Lines { get; set; } = new();

        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
