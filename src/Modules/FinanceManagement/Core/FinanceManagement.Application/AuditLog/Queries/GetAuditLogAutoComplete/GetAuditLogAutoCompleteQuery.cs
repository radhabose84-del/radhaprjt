using Contracts.Common;
using FinanceManagement.Application.AuditLog.Queries.GetAuditLog;
using MediatR;

namespace FinanceManagement.Application.AuditLog.Queries.GetAuditLogBySearchPattern
{
    public class GetAuditLogBySearchPatternQuery : IRequest<ApiResponseDTO<List<AuditLogDto>>>
    {
        public string? SearchPattern { get; set; }
    }
}
