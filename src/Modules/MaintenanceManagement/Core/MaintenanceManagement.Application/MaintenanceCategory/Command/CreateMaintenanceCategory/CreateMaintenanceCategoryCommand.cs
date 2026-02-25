using MediatR;

namespace MaintenanceManagement.Application.MaintenanceCategory.Command.CreateMaintenanceCategory
{
    public class CreateMaintenanceCategoryCommand :IRequest<int>
    {
        public string? CategoryName { get; set; }
        public string? Description { get; set; }
    }
}