using MediatR;

namespace MaintenanceManagement.Application.MaintenanceType.Command.CreateMaintenanceType
{
    public class CreateMaintenanceTypeCommand :IRequest<int>
    {
         public string? TypeName { get; set; }
    }
}