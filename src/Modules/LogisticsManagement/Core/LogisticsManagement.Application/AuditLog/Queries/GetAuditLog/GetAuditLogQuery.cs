using MediatR;

namespace LogisticsManagement.Application.AuditLog.Queries.GetAuditLog
{
    public class GetAuditLogQuery : IRequest<List<AuditLogDto>>;
}
