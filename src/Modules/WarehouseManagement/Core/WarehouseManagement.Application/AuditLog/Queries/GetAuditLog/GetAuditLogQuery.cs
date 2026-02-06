using MediatR;

namespace WarehouseManagement.Application.AuditLog.Queries.GetAuditLog
{   
   public class GetAuditLogQuery : IRequest<List<AuditLogDto>>;
          
}