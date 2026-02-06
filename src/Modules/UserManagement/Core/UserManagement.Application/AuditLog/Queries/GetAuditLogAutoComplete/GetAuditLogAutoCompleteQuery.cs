using UserManagement.Application.AuditLog.Queries.GetAuditLog;
using UserManagement.Application.Common.HttpResponse;
using MediatR;

namespace UserManagement.Application.AuditLog.Queries
{
    public class GetAuditLogBySearchPatternQuery : IRequest<ApiResponseDTO<List<AuditLogDto>>>
    {
        public string? SearchPattern { get; set; } 
    }
}
