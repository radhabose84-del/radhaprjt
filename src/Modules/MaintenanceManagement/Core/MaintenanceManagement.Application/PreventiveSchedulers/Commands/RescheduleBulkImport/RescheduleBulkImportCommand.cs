using Contracts.Common;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace MaintenanceManagement.Application.PreventiveSchedulers.Commands.RescheduleBulkImport
{
    public class RescheduleBulkImportCommand : IRequest<ApiResponseDTO<string>>, IRequirePermission
    {
        public PermissionType RequiredPermission => PermissionType.CanAdd;
        public IFormFile File { get; set; } = default!;
    }
}