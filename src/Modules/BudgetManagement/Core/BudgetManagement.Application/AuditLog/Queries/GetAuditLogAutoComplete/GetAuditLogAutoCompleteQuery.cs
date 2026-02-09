using BudgetManagement.Application.AuditLog.Queries.GetAuditLog;
using BudgetManagement.Application.Common.HttpResponse;
using MediatR;

namespace BudgetManagement.Application.AuditLog.Queries
{
    public class GetAuditLogBySearchPatternQuery : IRequest<ApiResponseDTO<List<AuditLogDto>>>
    {
        public string? SearchPattern { get; set; } 
    }
}
