using MediatR;

namespace MaintenanceManagement.Application.ActivityMaster.Command.UpdateActivityMster
{
    public class UpdateActivityMasterCommand  : IRequest<int>
    {

     public UpdateActivityMasterDto? UpdateActivityMaster  { get; set; }
    }
}