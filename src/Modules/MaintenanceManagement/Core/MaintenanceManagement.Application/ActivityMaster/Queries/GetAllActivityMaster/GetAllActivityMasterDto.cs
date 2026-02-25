using static MaintenanceManagement.Domain.Common.BaseEntity;

namespace MaintenanceManagement.Application.ActivityMaster.Queries.GetAllActivityMaster
{
    public class GetAllActivityMasterDto
    {
        public int Id { get; set;}
        public string? ActivityName { get; set;}
        public string? Description { get; set; }
        public int UnitId { get; set;}
        public string? UnitName { get; set; }
        public int DepartmentId { get; set; }
        public string? DepartmentName { get; set; }        
        public int EstimatedDuration { get; set; }
        public int ActivityType { get; set; }
        public string? ActivityTypeDescription { get; set; }        
        public Status  IsActive { get; set; }
        public DateTimeOffset CreatedDate { get; set; }

        
        
    }
}