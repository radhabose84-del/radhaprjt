using Contracts.Common;
using FinanceManagement.Application.ScheduleIII.Dto;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace FinanceManagement.Application.ScheduleIII.Commands.ImportScheduleIIIFile
{
    // Browse + upload an Excel/CSV file (multipart). Parsed then delegated to ImportScheduleIIICommand.
    public class ImportScheduleIIIFileCommand : IRequest<ApiResponseDTO<ImportScheduleIIIResultDto>>, IRequirePermission
    {
        public IFormFile? File { get; set; }

        public PermissionType RequiredPermission => PermissionType.CanAdd;
    }
}
