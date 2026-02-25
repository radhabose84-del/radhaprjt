using MediatR;

namespace MaintenanceManagement.Application.ActivityMaster.Command.CreateActivityMaster
{
    public class CreateActivityMasterCommand : IRequest<int>
    {

        public CreateActivityMasterDto? CreateActivityMasterDto { get; set; }
        


    }
}