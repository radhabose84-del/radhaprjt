using MediatR;

namespace PurchaseManagement.Application.AuditLog.Queries.GetAuditLog
{   
   public class GetAuditLogQuery : IRequest<List<AuditLogDto>>;
          
}