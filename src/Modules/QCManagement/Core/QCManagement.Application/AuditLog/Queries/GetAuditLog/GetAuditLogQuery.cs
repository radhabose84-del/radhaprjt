using MediatR;

namespace QCManagement.Application.AuditLog.Queries.GetAuditLog
{
    public class GetAuditLogQuery : IRequest<List<AuditLogDto>>
    {
    }
}
