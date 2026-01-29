using FAM.Application.AuditLog.Queries.GetAuditLog;
using FAM.Application.Common.HttpResponse;
using MediatR;

namespace FAM.Application.AuditLog.Queries
{
    public class GetAuditLogBySearchPatternQuery : IRequest<ApiResponseDTO<List<AuditLogDto>>>
    {
        public string? SearchPattern { get; set; } 
    }
}
