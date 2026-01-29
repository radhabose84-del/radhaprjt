using Core.Application.Common.Mappings;
using Core.Domain.Entities;

namespace Core.Application.AuditLog.Queries.GetAuditLog
{
    public class AuditLogDto : IMapFrom<AuditLogs>
    { 
        public string? Id { get; set; } // Use string for ObjectId        
        public string? MachineName { get; set; }
        public string? IPAddress { get; set; }
        public string? OS { get; set; }
        public string? Browser { get; set; }
        public string? Action { get; set; }
        public string? Details { get; set; }
        public string? Module { get; set; }
        public DateTime CreatedAt { get; set; }   
        public int CreatedBy { get; set; }
        public string? CreatedByName { get; set; }       
        
                   
    }
}