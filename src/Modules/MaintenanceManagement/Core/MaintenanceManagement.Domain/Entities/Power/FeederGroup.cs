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