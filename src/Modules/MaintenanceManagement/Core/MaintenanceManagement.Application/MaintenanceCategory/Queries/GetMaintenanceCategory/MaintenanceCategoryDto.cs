using static MaintenanceManagement.Domain.Common.BaseEntity;

namespace MaintenanceManagement.Application.MaintenanceCategory.Queries.GetMaintenanceCategory
{
    public class MaintenanceCategoryDto
    {
        public int Id { get; set; }
        public string? CategoryName { get; set; }
        public string? Description { get; set; }
        public Status IsActive { get; set; }
        public DateTimeOffset CreatedDate { get; set; }

    }
}