using MediatR;

namespace BudgetManagement.Application.AuditLog.Queries.GetAuditLog
{   
   public class GetAuditLogQuery : IRequest<List<AuditLogDto>>;
          
}