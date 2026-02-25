using MediatR;

namespace MaintenanceManagement.Application.ActivityCheckListMaster.Command.CreateActivityCheckListMaster
{
    public class CreateActivityCheckListMasterCommand : IRequest<int>
    {

        public int ActivityID { get; set; }
        public string? ActivityCheckList { get; set; }       
        public int  UnitId { get; set; }

    }
}