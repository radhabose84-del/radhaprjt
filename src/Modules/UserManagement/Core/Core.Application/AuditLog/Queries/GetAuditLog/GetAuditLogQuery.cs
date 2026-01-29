using MediatR;

namespace Core.Application.AuditLog.Queries.GetAuditLog
{   
   public class GetAuditLogQuery : IRequest<List<AuditLogDto>>;
          
}