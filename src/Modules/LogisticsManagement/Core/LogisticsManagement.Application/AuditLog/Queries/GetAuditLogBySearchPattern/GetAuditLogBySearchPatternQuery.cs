using LogisticsManagement.Application.AuditLog.Queries.GetAuditLog;
using Contracts.Common;
using MediatR;

namespace LogisticsManagement.Application.AuditLog.Queries
{
    public class GetAuditLogBySearchPatternQuery : IRequest<ApiResponseDTO<List<AuditLogDto>>>
    {
        public string? SearchPattern { get; set; }
    }
}
