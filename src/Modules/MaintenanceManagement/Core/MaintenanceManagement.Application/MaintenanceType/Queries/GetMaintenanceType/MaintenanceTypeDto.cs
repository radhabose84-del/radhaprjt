using static MaintenanceManagement.Domain.Common.BaseEntity;

namespace MaintenanceManagement.Application.MaintenanceType.Queries.GetMaintenanceType
{
    public class MaintenanceTypeDto
    {
        public int Id { get; set; }
        public string? TypeName { get; set; }
        public Status IsActive { get; set; }
        public DateTimeOffset CreatedDate { get; set; }

        
    }
}