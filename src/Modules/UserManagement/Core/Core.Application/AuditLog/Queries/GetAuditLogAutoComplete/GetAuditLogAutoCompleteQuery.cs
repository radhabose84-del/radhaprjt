using Core.Application.AuditLog.Queries.GetAuditLog;
using Core.Application.Common.HttpResponse;
using MediatR;

namespace Core.Application.AuditLog.Queries
{
    public class GetAuditLogBySearchPatternQuery : IRequest<ApiResponseDTO<List<AuditLogDto>>>
    {
        public string? SearchPattern { get; set; } 
    }
}
