using MediatR;

namespace MaintenanceManagement.Application.MaintenanceType.Command.UpdateMaintenanceType
{
    public class UpdateMaintenanceTypeCommand :IRequest<int>
    {
        public int Id {get;set;}
        public string? TypeName { get; set; }
        public byte IsActive { get; set; }
    }
}