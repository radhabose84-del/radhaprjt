using SalesManagement.Application.AuditLog.Queries.GetAuditLog;
using SalesManagement.Application.Common.HttpResponse;
using MediatR;

namespace SalesManagement.Application.AuditLog.Queries
{
    public class GetAuditLogBySearchPatternQuery : IRequest<ApiResponseDTO<List<AuditLogDto>>>
    {
        public string? SearchPattern { get; set; } 
    }
}
