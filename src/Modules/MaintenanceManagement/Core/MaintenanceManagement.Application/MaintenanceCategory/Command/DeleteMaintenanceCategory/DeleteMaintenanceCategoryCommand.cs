using MediatR;

namespace MaintenanceManagement.Application.MaintenanceCategory.Command.DeleteMaintenanceCategory
{
    public class DeleteMaintenanceCategoryCommand : IRequest<int> 
    {
        public int Id { get; set; }
    }
}