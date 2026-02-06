using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WarehouseManagement.Domain.Entities;

namespace WarehouseManagement.Application.Common.Interfaces.AuditLog
{
    public interface IAuditLogRepository
    {   
        Task<AuditLogs> CreateAsync(AuditLogs auditLog);                
        Task<List<AuditLogs>> GetAllAsync();    
        Task<List<AuditLogs>> GetByAuditLogNameAsync(string auditLogName);               
    }

}