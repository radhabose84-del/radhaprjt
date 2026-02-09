using ProjectManagement.Application.AuditLog.Queries.GetAuditLog;
using ProjectManagement.Application.Common.HttpResponse;
using MediatR;

namespace ProjectManagement.Application.AuditLog.Queries
{
    public class GetAuditLogBySearchPatternQuery : IRequest<ApiResponseDTO<List<AuditLogDto>>>
    {
        public string? SearchPattern { get; set; } 
    }
}
