using Contracts.Common;
using QCManagement.Application.AuditLog.Queries.GetAuditLog;
using MediatR;

namespace QCManagement.Application.AuditLog.Queries.GetAuditLogAutoComplete
{
    public class GetAuditLogAutoCompleteQuery : IRequest<ApiResponseDTO<List<AuditLogDto>>>
    {
        public string? SearchPattern { get; set; }
    }
}
