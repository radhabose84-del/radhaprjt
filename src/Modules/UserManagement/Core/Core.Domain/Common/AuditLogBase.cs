using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Domain.Common
{
    public abstract class AuditLogBase
    {
        public string? MachineName { get; set; }
        public string? IPAddress { get; set; }
        public string? OS { get; set; }
        public string? Browser { get; set; }
        public DateTime CreatedAt { get; set; }   
        public int CreatedBy { get; set; }
        public String? CreatedByName { get; set; }
    }
}