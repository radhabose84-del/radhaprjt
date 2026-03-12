using MediatR;

namespace FinanceManagement.Application.AuditLog.Queries.GetAuditLog
{
    public class GetAuditLogQuery : IRequest<List<AuditLogDto>>
    {
    }
}
