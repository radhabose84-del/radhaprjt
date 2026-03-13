using Contracts.Common;
using MediatR;
using ProductionManagement.Application.AuditLog.Queries.GetAuditLog;

namespace ProductionManagement.Application.AuditLog.Queries.GetAuditLogAutoComplete
{
    public class GetAuditLogAutoCompleteQuery : IRequest<ApiResponseDTO<List<AuditLogDto>>>
    {
        public string? SearchPattern { get; set; }
    }
}
