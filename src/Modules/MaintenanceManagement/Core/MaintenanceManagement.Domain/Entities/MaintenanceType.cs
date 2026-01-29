using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Domain.Common;

namespace MaintenanceManagement.Domain.Entities
{
    public class MaintenanceType :BaseEntity
    {
        public string? TypeName { get; set; }
    }
}