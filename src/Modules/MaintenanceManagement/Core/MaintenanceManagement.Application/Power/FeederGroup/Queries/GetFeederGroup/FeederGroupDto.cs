using static MaintenanceManagement.Domain.Common.BaseEntity;

namespace MaintenanceManagement.Application.Power.FeederGroup.Queries.GetFeederGroup
{
    public class FeederGroupDto
    {
          public int Id { get; set; }

         public string? FeederGroupCode { get; set; }  
        public string? FeederGroupName { get; set; }
        public int  UnitId { get; set; }
        public Status IsActive { get; set; }
    }
}