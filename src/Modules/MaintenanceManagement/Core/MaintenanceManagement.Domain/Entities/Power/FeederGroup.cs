using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Domain.Common;

namespace MaintenanceManagement.Domain.Entities.Power
{
    public class FeederGroup : BaseEntity
    {

        public string? FeederGroupCode { get; set; }

        public string? FeederGroupName { get; set; }

        public int  UnitId { get; set; }

        public ICollection<Feeder>? Feeders { get; set; }
        
        
    }
}