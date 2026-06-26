using Contracts.Common;
using FinanceManagement.Application.ScheduleIII.Dto;
using MediatR;

namespace FinanceManagement.Application.ScheduleIII.Commands.ImportScheduleIII
{
    // Bulk import of Schedule III sections + their line items from parsed rows (all-or-nothing).
    public class ImportScheduleIIICommand : IRequest<ApiResponseDTO<ImportScheduleIIIResultDto>>, IRequirePermission
    {
        public string? FileName { get; set; }
        public List<ScheduleIIIImportRowInputDto> Rows { get; set; } = new();

        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
