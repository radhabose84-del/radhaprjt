using PartyManagement.Application.AuditLog.Queries.GetAuditLog;
using PartyManagement.Application.Common.HttpResponse;
using MediatR;

namespace PartyManagement.Application.AuditLog.Queries
{
    public class GetAuditLogBySearchPatternQuery : IRequest<ApiResponseDTO<List<AuditLogDto>>>
    {
        public string? SearchPattern { get; set; } 
    }
}
