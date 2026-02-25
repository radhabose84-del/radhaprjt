using MediatR;

namespace MaintenanceManagement.Application.MachineMaster.Command.DeleteMachineMaster
{
    public class DeleteMachineMasterCommand : IRequest<bool>
    {
        public int Id { get; set; } 
    }
}