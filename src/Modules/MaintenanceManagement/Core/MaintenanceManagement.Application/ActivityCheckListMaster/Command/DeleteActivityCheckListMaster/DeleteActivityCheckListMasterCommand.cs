using MediatR;

namespace MaintenanceManagement.Application.ActivityCheckListMaster.Command.DeleteActivityCheckListMaster
{
    public class DeleteActivityCheckListMasterCommand : IRequest<bool> 
    {
          public int Id { get; set; }
    }
}