using Contracts.Common;
using GateEntryManagement.Application.AuditLog.Queries.GetAuditLog;
using MediatR;

namespace GateEntryManagement.Application.AuditLog.Queries
{
    public class GetAuditLogBySearchPatternQuery : IRequest<ApiResponseDTO<List<AuditLogDto>>>
    {
        public string? SearchPattern { get; set; }
    }
}
