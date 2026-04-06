using LogisticsManagement.Domain.Entities;

namespace LogisticsManagement.Application.Common.Interfaces.AuditLog
{
    public interface IAuditLogRepository
    {
        Task<AuditLogs> CreateAsync(AuditLogs auditLog);
        Task<List<AuditLogs>> GetAllAsync();
        Task<List<AuditLogs>> GetByAuditLogNameAsync(string auditLogName);
    }
}
