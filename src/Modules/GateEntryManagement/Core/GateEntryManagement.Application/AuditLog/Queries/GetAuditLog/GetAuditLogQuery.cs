using MediatR;

namespace GateEntryManagement.Application.AuditLog.Queries.GetAuditLog
{
    public class GetAuditLogQuery : IRequest<List<AuditLogDto>>
    {
    }
}
