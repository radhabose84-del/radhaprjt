using PartyManagement.Domain.Entities;

namespace PartyManagement.Application.Common.Interfaces.AuditLog
{
    public interface IAuditLogRepository
    {   
        Task<AuditLogs> CreateAsync(AuditLogs auditLog);                
        Task<List<AuditLogs>> GetAllAsync();    
        Task<List<AuditLogs>> GetByAuditLogNameAsync(string auditLogName);               
    }

}