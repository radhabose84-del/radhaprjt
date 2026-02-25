using MediatR;

namespace MaintenanceManagement.Application.MaintenanceCategory.Command.UpdateMaintenanceCategory
{
    public class UpdateMaintenanceCategoryCommand  :IRequest<int>
    {
        public int Id {get;set;}
        public string? CategoryName { get; set; }
        public string? Description { get; set; }

        public byte IsActive { get; set; }
    }
}