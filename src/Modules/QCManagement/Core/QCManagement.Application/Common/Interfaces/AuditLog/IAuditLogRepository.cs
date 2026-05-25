using QCManagement.Domain.Entities;

namespace QCManagement.Application.Common.Interfaces.AuditLog
{
    public interface IAuditLogRepository
    {
        Task<AuditLogs> CreateAsync(AuditLogs auditLog);
        Task<List<AuditLogs>> GetAllAsync();
        Task<List<AuditLogs>> GetByAuditLogNameAsync(string searchPattern);
    }
}
