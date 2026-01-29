using MaintenanceManagement.Application.AuditLog.Queries.GetAuditLog;
using MaintenanceManagement.Application.Common.HttpResponse;
using MediatR;

namespace MaintenanceManagement.Application.AuditLog.Queries
{
    public class GetAuditLogBySearchPatternQuery : IRequest<ApiResponseDTO<List<AuditLogDto>>>
    {
        public string? SearchPattern { get; set; } 
    }
}
