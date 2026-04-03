using MediatR;

namespace MaintenanceManagement.Application.ActivityMaster.Command.DeleteActivityMaster
{
    public class DeleteActivityMasterCommand : IRequest<bool>
    {
         public int Id { get; set; }
    }
}
