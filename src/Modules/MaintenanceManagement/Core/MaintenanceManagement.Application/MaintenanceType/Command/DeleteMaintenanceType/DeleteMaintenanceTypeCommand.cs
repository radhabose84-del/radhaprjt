using MediatR;

namespace MaintenanceManagement.Application.MaintenanceType.Command.DeleteMaintenanceType
{
    public class DeleteMaintenanceTypeCommand : IRequest<int>
    {
        public int Id { get; set; } 
    }
}