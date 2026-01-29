using MediatR;

namespace MaintenanceManagement.Application.AuditLog.Queries.GetAuditLog
{   
   public class GetAuditLogQuery : IRequest<List<AuditLogDto>>;
          
}