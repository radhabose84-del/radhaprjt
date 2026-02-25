using MediatR;

namespace MaintenanceManagement.Application.ActivityMaster.Queries.GetMachineGroupById
{
    public class GetMachineGroupNameByIdQuery   :   IRequest<List<GetMachineGroupNameByIdDto>>
    {
       public int   ActivityId { get; set; }    
        
    }
}